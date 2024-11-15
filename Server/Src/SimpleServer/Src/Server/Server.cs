using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Network;
using Lockstep.Util;
using NetMsg.Common;

namespace Lockstep.FakeServer
{
    public class Server : IMessageDispatcher
    {
        //network
        public static IPEndPoint serverIpPoint = NetworkUtil.ToIPEndPoint("127.0.0.1", 10083);
        private NetOuterProxy _netProxy = new NetOuterProxy();

        //update
        /// <summary>
        /// 服务器更新帧间隔
        /// </summary>
        private const double UpdateInterval = NetworkDefine.UPDATE_DELTATIME / 1000.0f; //frame rate = 30
        /// <summary>
        /// 下一次更新时间
        /// </summary>
        private DateTime _lastUpdateTimeStamp;
        /// <summary>
        /// 服务器开启时
        /// </summary>
        private DateTime _startUpTimeStamp;
        /// <summary>
        /// 上一帧到当前帧间隔时间
        /// </summary>
        private double _deltaTime;
        /// <summary>
        /// 服务器总运行时长
        /// </summary>
        private double _timeSinceStartUp;

        //user mgr 
        private GameManager _gameManger = new GameManager();
        private List<Player> waitPlayers = new List<Player>();


        private Dictionary<long, Player> _id2Player = new Dictionary<long, Player>();

        //id
        private static int _idCounter = 0;
        private int _curCount = 0;


        public void Start()
        {
            _netProxy.MessageDispatcher = this;
            _netProxy.MessagePacker = MessagePacker.Instance;
            _netProxy.Awake(NetworkProtocol.TCP, serverIpPoint);
            _startUpTimeStamp = _lastUpdateTimeStamp = DateTime.Now;
        }

        public void Dispatch(Session session, Packet packet)
        {
            //消息解析
            ushort opcode = packet.Opcode();
            var message = session.Network.MessagePacker.DeserializeFrom(opcode, packet.Bytes, Packet.Index,
                packet.Length - Packet.Index);
            OnNetMsg(session, opcode, message as BaseMsg);
        }

        void OnNetMsg(Session session, ushort opcode, BaseMsg msg)
        {
            var type = (EMsgSC)opcode;
            switch (type)
            {
                //login
                // case EMsgSC.L2C_JoinRoomResult: 
                case EMsgSC.C2L_JoinRoom:
                    OnPlayerConnect(session, msg);
                    return;
                case EMsgSC.C2L_LeaveRoom:
                    OnPlayerQuit(session, msg);
                    return;
                    //room
            }
            var player = session.GetBindInfo<Player>();
            player.Game.OnNetMsg(player, opcode, msg);
        }

        public void Update()
        {
            var now = DateTime.Now;
            _deltaTime = (now - _lastUpdateTimeStamp).TotalSeconds;
            if (_deltaTime > UpdateInterval)
            {
                _lastUpdateTimeStamp = now;
                _timeSinceStartUp = (now - _startUpTimeStamp).TotalSeconds;
                DoUpdate();
            }
        }

        public void DoUpdate()
        {
            //check frame inputs
            var fDeltaTime = (float)_deltaTime;
            var fTimeSinceStartUp = (float)_timeSinceStartUp;
            _gameManger.DoUpdate(fDeltaTime);
        }

        /// <summary>
        /// 玩家进入房间
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message"></param>
        void OnPlayerConnect(Session session, BaseMsg message)
        {
            var info = new Player();
            info.UserId = _idCounter++;
            info.PeerTcp = session;
            info.PeerUdp = session;
            _id2Player[info.UserId] = info;
            session.BindInfo = info;
            _curCount++;
            waitPlayers.Add(info);
            if (waitPlayers.Count >= Game.MaxPlayerCount)
            {
                _gameManger.CreateGame(waitPlayers.ToArray());
                waitPlayers.Clear();
            }
            Debug.Log("OnPlayerConnect count:" + _curCount + " ");
        }
        /// <summary>
        /// 玩家退出
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message"></param>
        void OnPlayerQuit(Session session, BaseMsg message)
        {
            var player = session.GetBindInfo<Player>();
            if (player == null)
                return;
            _curCount--;
            _id2Player.Remove(player.UserId);
            _gameManger.OnPlayerQuit(player);
            Debug.Log("OnPlayerQuit count:" + _curCount);
        }
    }
}