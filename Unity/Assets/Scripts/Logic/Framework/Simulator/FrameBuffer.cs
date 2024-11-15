#define DEBUG_FRAME_DELAY
using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Math;
using Lockstep.Serialization;
using Lockstep.Util;
using NetMsg.Common;
using Debug = Lockstep.Logging.Debug;

namespace Lockstep.Game
{
    public interface IFrameBuffer
    {
        void ForcePushDebugFrame(ServerFrame frame);
        void PushLocalFrame(ServerFrame frame);
        void PushServerFrames(ServerFrame[] frames, bool isNeedDebugCheck = true);
        void PushMissServerFrames(ServerFrame[] frames, bool isNeedDebugCheck = true);
        void OnPlayerPing(Msg_G2C_PlayerPing msg);
        ServerFrame GetFrame(int tick);
        ServerFrame GetServerFrame(int tick);
        ServerFrame GetLocalFrame(int tick);
        void SetClientTick(int tick);
        void SendInput(Msg_PlayerInput input);

        void DoUpdate(float deltaTime);
        int NextTickToCheck { get; }
        int MaxServerTickInBuffer { get; }
        bool IsNeedRollback { get; }
        /// <summary>
        /// 服务器最大连续帧 小于服务器最大帧 说明有丢帧
        /// </summary>
        int MaxContinueServerTick { get; }
        /// <summary>
        /// 收到服务器最大的帧号 不一定存入了列表 不一定连续
        /// </summary>
        int CurTickInServer { get; }
        int PingVal { get; }
        int DelayVal { get; }
    }

    public class FrameBuffer : IFrameBuffer
    {
        public class PredictCountHelper
        {
            public PredictCountHelper(SimulatorService simulatorService, FrameBuffer cmdBuffer)
            {
                this._cmdBuffer = cmdBuffer;
                this._simulatorService = simulatorService;
            }

            public int missTick = -1;
            public int nextCheckMissTick = 0;
            public bool hasMissTick;

            private SimulatorService _simulatorService;
            private FrameBuffer _cmdBuffer;
            private float _timer;
            private float _checkInterval = 0.5f;
            private float _incPercent = 0.3f;

            private float _targetPreSendTick;
            private float _oldPercent = 0.6f;

            public void DoUpdate(float deltaTime)
            {
                _timer += deltaTime;
                if (_timer > _checkInterval)
                {
                    _timer = 0;
                    if (!hasMissTick)
                    {
                        var preSend = _cmdBuffer._maxPing * 1.0f / NetworkDefine.UPDATE_DELTATIME;
                        _targetPreSendTick = _targetPreSendTick * _oldPercent + preSend * (1 - _oldPercent);

                        var targetPreSendTick = LMath.Clamp((int)System.Math.Ceiling(_targetPreSendTick), 1, 60);
#if UNITY_EDITOR
                        //if (targetPreSendTick != _simulatorService.PreSendInputCount) 
                        {
                            Debug.LogWarning(
                                $"Shrink preSend buffer old:{_simulatorService.PreSendInputCount} new:{_targetPreSendTick} " +
                                $"PING: min:{_cmdBuffer._minPing} max:{_cmdBuffer._maxPing} avg:{_cmdBuffer.PingVal}");
                        }
#endif
                        _simulatorService.PreSendInputCount = targetPreSendTick;
                    }

                    hasMissTick = false;
                }

                if (missTick != -1)
                {
                    var delayTick = _simulatorService.TargetTick - missTick;
                    var targetPreSendTick =
                        _simulatorService.PreSendInputCount + (int)System.Math.Ceiling(delayTick * _incPercent);
                    targetPreSendTick = LMath.Clamp(targetPreSendTick, 1, 60);
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"Expend preSend buffer old:{_simulatorService.PreSendInputCount} new:{targetPreSendTick}");
#endif
                    _simulatorService.PreSendInputCount = targetPreSendTick;
                    nextCheckMissTick = _simulatorService.TargetTick;
                    missTick = -1;
                    hasMissTick = true;
                }
            }
        }

        /// for debug
        public static byte __debugMainActorID;

        //buffers
        private int _maxClientPredictFrameCount;
        private int _bufferSize;
        private int _spaceRollbackNeed;
        /// <summary>
        /// ？最多覆盖多少帧
        /// </summary>
        private int _maxServerOverFrameCount;

        private ServerFrame[] _serverBuffer;
        private ServerFrame[] _clientBuffer;

        /// <summary>
        /// ping延迟平均值
        /// </summary>
        public int PingVal { get; private set; }
        /// <summary>
        /// 没帧收到的ping数据
        /// </summary>
        private List<long> _pings = new List<long>();
        /// <summary>
        /// 客户端运行时长 - 服务器游戏开始时间(收到所有玩家第0帧帧数据) - ping/2
        /// </summary>
        private long _guessServerStartTimestamp = Int64.MaxValue;
        /// <summary>
        /// 历史ping最小值
        /// </summary>
        private long _historyMinPing = Int64.MaxValue;
        private long _minPing = Int64.MaxValue;
        private long _maxPing = Int64.MinValue;
        /// <summary>
        /// 帧数据延迟平均值
        /// </summary>
        public int DelayVal { get; private set; }
        private float _pingTimer;
        /// <summary>
        /// 帧数据延迟时间 （收到帧 - 发送帧） 
        /// </summary>
        private List<long> _delays = new List<long>();
        /// <summary>
        /// 发送帧时的时间戳 用来计算延迟
        /// </summary>
        Dictionary<int, long> _tick2SendTimestamp = new Dictionary<int, long>();

        /// the tick client need run in next update
        private int _nextClientTick;
        public int CurTickInServer { get; private set; }
        /// <summary>
        /// 下一个需要验证的帧 前面的都验证完了
        /// </summary>
        public int NextTickToCheck { get; private set; }
        /// <summary>
        /// 收到服务器最大的帧号并存入列表 前面可能存在丢帧
        /// </summary>
        public int MaxServerTickInBuffer { get; private set; } = -1;
        /// <summary>
        /// 有数据不一致 需要回滚
        /// </summary>
        public bool IsNeedRollback { get; private set; }
        public int MaxContinueServerTick { get; private set; }

        public byte LocalId;

        public INetworkService _networkService;

        private PredictCountHelper _predictHelper;
        private SimulatorService _simulatorService;
        /// <summary>
        /// FrameBuffer
        /// </summary>
        /// <param name="_simulatorService"></param>
        /// <param name="networkService"></param>
        /// <param name="bufferSize">最多存多少帧的数据</param>
        /// <param name="snapshotFrameInterval">快照间隔 默认为1</param>
        /// <param name="maxClientPredictFrameCount">最多预测多少帧</param>
        public FrameBuffer(SimulatorService _simulatorService, INetworkService networkService, int bufferSize,
            int snapshotFrameInterval,
            int maxClientPredictFrameCount)
        {
            this._simulatorService = _simulatorService;
            _predictHelper = new PredictCountHelper(_simulatorService, this);
            this._bufferSize = bufferSize;
            this._networkService = networkService;
            this._maxClientPredictFrameCount = maxClientPredictFrameCount;
            _spaceRollbackNeed = snapshotFrameInterval * 2;
            _maxServerOverFrameCount = bufferSize - _spaceRollbackNeed;
            _serverBuffer = new ServerFrame[bufferSize];
            _clientBuffer = new ServerFrame[bufferSize];
        }

        public void SetClientTick(int tick)
        {
            _nextClientTick = tick + 1;
        }

        public void PushLocalFrame(ServerFrame frame)
        {
            var sIdx = frame.tick % _bufferSize;
            Debug.Assert(_clientBuffer[sIdx] == null || _clientBuffer[sIdx].tick <= frame.tick,
                "Push local frame error!");
            _clientBuffer[sIdx] = frame;
        }

        /// <summary>
        /// 服务器下发Ping数据
        /// </summary>
        /// <param name="msg"></param>
        public void OnPlayerPing(Msg_G2C_PlayerPing msg)
        {
            //PushServerFrames(frames, isNeedDebugCheck);
            var ping = LTime.realtimeSinceStartupMS - msg.sendTimestamp;
            _pings.Add(ping);
            if (ping > _maxPing) _maxPing = ping;
            if (ping < _minPing)
            {
                _minPing = ping;
                _guessServerStartTimestamp = (LTime.realtimeSinceStartupMS - msg.timeSinceServerStart) - _minPing / 2;
            }

            //Debug.Log("OnPlayerPing " + ping);
        }

        public void PushMissServerFrames(ServerFrame[] frames, bool isNeedDebugCheck = true)
        {
            PushServerFrames(frames, isNeedDebugCheck);
            _networkService.SendMissFrameRepAck(MaxContinueServerTick + 1);
        }

        public void ForcePushDebugFrame(ServerFrame data)
        {
            var targetIdx = data.tick % _bufferSize;
            _serverBuffer[targetIdx] = data;
            _clientBuffer[targetIdx] = data;
        }
        /// <summary>
        /// 收到服务器帧数据
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="isNeedDebugCheck"></param>
        public void PushServerFrames(ServerFrame[] frames, bool isNeedDebugCheck = true)
        {
            var count = frames.Length;
            for (int i = 0; i < count; i++)
            {
                var data = frames[i];
                //Debug.Log("PushServerFrames" + data.tick);
                if (_tick2SendTimestamp.TryGetValue(data.tick, out var sendTick))
                {
                    var delay = LTime.realtimeSinceStartupMS - sendTick;
                    _delays.Add(delay);
                    _tick2SendTimestamp.Remove(data.tick);
                }

                if (data.tick < NextTickToCheck)
                {
                    //the frame is already checked 当前帧已经验证过了
                    return;
                }

                if (data.tick > CurTickInServer)
                {
                    CurTickInServer = data.tick;
                }
                //？？？？
                if (data.tick >= NextTickToCheck + _maxServerOverFrameCount - 1)
                {
                    //to avoid ringBuffer override the frame that have not been checked
                    return;
                }

                //Debug.Log("PushServerFramesSucc" + data.tick);
                if (data.tick > MaxServerTickInBuffer)
                {
                    MaxServerTickInBuffer = data.tick;
                }

                var targetIdx = data.tick % _bufferSize;
                if (_serverBuffer[targetIdx] == null || _serverBuffer[targetIdx].tick != data.tick)
                {
                    _serverBuffer[targetIdx] = data;
                    //？？？？
                    if (data.tick > _predictHelper.nextCheckMissTick && data.Inputs[LocalId].IsMiss &&
                        _predictHelper.missTick == -1)
                    {
                        _predictHelper.missTick = data.tick;
                    }
                }
            }
        }

        /// <summary>
        /// unity update
        /// </summary>
        /// <param name="deltaTime"></param>
        public void DoUpdate(float deltaTime)
        {
            _networkService.SendPing(_simulatorService.LocalActorId, LTime.realtimeSinceStartupMS);//？没帧都在发ping 是不是太密集了
            _predictHelper.DoUpdate(deltaTime);
            int worldTick = _simulatorService.World.Tick;
            UpdatePingVal(deltaTime);

            //Debug.Assert(nextTickToCheck <= nextClientTick, "localServerTick <= localClientTick ");
            //Confirm frames 验证帧
            IsNeedRollback = false;
            while (NextTickToCheck <= MaxServerTickInBuffer && NextTickToCheck < worldTick)
            {
                var sIdx = NextTickToCheck % _bufferSize;
                var cFrame = _clientBuffer[sIdx];
                var sFrame = _serverBuffer[sIdx];
                if (cFrame == null || cFrame.tick != NextTickToCheck || sFrame == null ||
                    sFrame.tick != NextTickToCheck)
                    break;//没有可验证的数据了 跳出循环 不需要回滚
                //Check client guess input match the real input 预测和服务器一致
                if (object.ReferenceEquals(sFrame, cFrame) || sFrame.Equals(cFrame))
                {
                    NextTickToCheck++;
                }
                else
                {
                    IsNeedRollback = true;//不一致 跳出循环 需要回滚
                    break;
                }
            }

            //Request miss frame data
            int tick = NextTickToCheck;
            for (; tick <= MaxServerTickInBuffer; tick++)
            {
                var idx = tick % _bufferSize;
                if (_serverBuffer[idx] == null || _serverBuffer[idx].tick != tick)
                {
                    break;
                }
            }

            MaxContinueServerTick = tick - 1;
            if (MaxContinueServerTick <= 0) return;
            if (MaxContinueServerTick < CurTickInServer // has some middle frame pack was lost 有丢帧
                || _nextClientTick >
                MaxContinueServerTick + (_maxClientPredictFrameCount - 3) //client has predict too much 客户的预测太远了
            )
            {
                Debug.Log("SendMissFrameReq " + MaxContinueServerTick);
                _networkService.SendMissFrameReq(MaxContinueServerTick);//请求丢失的帧数据
            }
        }

        private void UpdatePingVal(float deltaTime)
        {
            _pingTimer += deltaTime;
            if (_pingTimer > 0.5f)
            {
                _pingTimer = 0;
                DelayVal = (int)(_delays.Sum() / LMath.Max(_delays.Count, 1));
                _delays.Clear();
                PingVal = (int)(_pings.Sum() / LMath.Max(_pings.Count, 1));
                _pings.Clear();

                if (_minPing < _historyMinPing && _simulatorService._gameStartTimestampMs != -1)
                {
                    _historyMinPing = _minPing;
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"Recalc _gameStartTimestampMs {_simulatorService._gameStartTimestampMs} _guessServerStartTimestamp:{_guessServerStartTimestamp}");
#endif
                    //修正游戏开始时间
                    _simulatorService._gameStartTimestampMs = LMath.Min(_guessServerStartTimestamp,
                        _simulatorService._gameStartTimestampMs);
                }

                _minPing = Int64.MaxValue;
                _maxPing = Int64.MinValue;
            }
        }

        public void SendInput(Msg_PlayerInput input)
        {
            _tick2SendTimestamp[input.Tick] = LTime.realtimeSinceStartupMS;
#if DEBUG_SHOW_INPUT
            var cmd = input.Commands[0];
            var playerInput = new Deserializer(cmd.content).Parse<Lockstep.Game. PlayerInput>();
            if (playerInput.inputUV != LVector2.zero) {
                Debug.Log($"SendInput tick:{input.Tick} uv:{playerInput.inputUV}");
            }
#endif
            _networkService.SendInput(input);
        }

        public ServerFrame GetFrame(int tick)
        {
            var sFrame = GetServerFrame(tick);
            if (sFrame != null)
            {
                return sFrame;
            }

            return GetLocalFrame(tick);
        }

        public ServerFrame GetServerFrame(int tick)
        {
            if (tick > MaxServerTickInBuffer)
            {
                return null;
            }

            return _GetFrame(_serverBuffer, tick);
        }

        public ServerFrame GetLocalFrame(int tick)
        {
            if (tick >= _nextClientTick)
            {
                return null;
            }

            return _GetFrame(_clientBuffer, tick);
        }

        private ServerFrame _GetFrame(ServerFrame[] buffer, int tick)
        {
            var idx = tick % _bufferSize;
            var frame = buffer[idx];
            if (frame == null) return null;
            if (frame.tick != tick) return null;
            return frame;
        }
    }
}