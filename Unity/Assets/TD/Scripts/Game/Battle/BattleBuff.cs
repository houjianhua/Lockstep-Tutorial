using Lockstep.Logging;
using TDFramework;

namespace TD
{
    public class BattleBuff : ILife
    {
        public BuffConfig buffConfig;
        public BattleActor actor;
        private int buffTimer;
        public bool isEnd;
        private int idx;

        public void Init()
        {
            buffTimer = 0;
            isEnd = false;
            idx = 0;
        }

        public void Release()
        {
            actor = null;
        }

        public void Tick(int delta)
        {
            if (isEnd) return;
            buffTimer += delta;
            if (buffConfig.Interval == 0)
            {
                if (idx == 0)
                {
                    Done();
                    idx++;
                }
            }
            else if (buffConfig.Interval * idx >= buffTimer)
            {
                Done();
                idx++;
            }

            if (buffTimer >= buffConfig.Life)
            {
                isEnd = true;
                End();
            }
        }

        private void Done()
        {
            switch ((BuffEffectType)buffConfig.Type)
            {
                case BuffEffectType.AddHp:
                    actor.attribuite.AddHp(buffConfig.Value);
                    break;
                case BuffEffectType.Dizz:
                    actor.AddTag(BattleTagType.DontMove, TagTrue);
                    actor.AddTag(BattleTagType.DontUseSkill, TagTrue);
                    break;
                case BuffEffectType.SubMoveSpeed:
                    actor.attribuite.ChangeAttribuite(AttribuiteType.MoveSpeed, AttributeCalcType.AffterRatio, -buffConfig.Value);
                    break;
                default:
                    Debug.LogWarning("other buff effect type :", buffConfig.Type);
                    break;
            }
        }

        private void End()
        {
            switch ((BuffEffectType)buffConfig.Type)
            {
                case BuffEffectType.AddHp:
                    actor.attribuite.AddHp(buffConfig.Value);
                    break;
                case BuffEffectType.Dizz:
                    actor.RemoveTag(BattleTagType.DontMove, TagTrue);
                    actor.RemoveTag(BattleTagType.DontUseSkill, TagTrue);
                    break;
                case BuffEffectType.SubMoveSpeed:
                    actor.attribuite.ChangeAttribuite(AttribuiteType.MoveSpeed, AttributeCalcType.AffterRatio, buffConfig.Value);
                    break;
                default:
                    break;
            }
        }
        private bool TagTrue()
        {
            return true;
        }
        private bool TagFalse()
        {
            return false;
        }
    }
}