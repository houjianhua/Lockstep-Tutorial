using System;
using Lockstep.Math;
using TDFramework;

namespace TD
{
    public interface IBattleSkillHandler
    {
        void OnSkillTriggerHit(BattleSkill battleSkill, BattleSkillEffect effect);
        BattleActor actor { get; }
    }

    //技能
    public class BattleSkill : ILife
    {
        public IBattleSkillHandler handler;
        public int configId;
        public SkillConfig skillConfig => ConfigManager.Instance.GetConfig<SkillConfig>(configId);
        private int cd;
        public bool isSkilling { get; private set; }
        private int skillTimer;
        private BattleSkillEffect[] effects;
        public void Init()
        {
            cd = 0;
            isSkilling = false;
        }

        public void Release()
        {
            handler = null;
        }

        public void Tick(int delta)
        {
            if (cd > 0)
            {
                cd -= delta;
                if (cd < 0) cd = 0;
            }
            if (isSkilling)
            {
                skillTimer++;
                foreach (var effect in effects)
                {
                    var b = effect.Trigger(skillTimer, this);
                    if (b)
                    {
                        handler.OnSkillTriggerHit(this, effect);
                    }
                }
                if (skillTimer >= skillConfig.LifeTime)
                {
                    isSkilling = false;
                    handler.actor.RemoveTag(BattleTagType.DontMove, SkillDontMove);
                }
            }
        }

        public bool CanUse(BattleActor actor)
        {
            if (cd < 0) return false;
            if (actor.attribuite.mp < skillConfig.Mp) return false;
            return true;
        }

        public void StartUse()
        {
            if (isSkilling) return;
            isSkilling = true;
            skillTimer = 0;
            if (effects == null)
            {
                effects = new BattleSkillEffect[skillConfig.effects.Length];
                for (int i = 0; i < skillConfig.effects.Length; i++)
                {
                    effects[i] = new BattleSkillEffect(skillConfig.effects[i]);
                }
            }
            else
            {
                foreach (var effect in effects)
                {
                    effect.Reset();
                }
            }
            handler.actor.AddTag(BattleTagType.DontMove, SkillDontMove);
        }

        public bool SkillDontMove()
        {
            return true;
        }

        public static bool ChackInUseRange(BattleActor attacker, BattleActor target, BattleSkill skill)
        {
            if (skill == null) return false;
            if (attacker == target) return false;
            if (attacker.compType == target.compType) return false;
            if (target.attribuite.hp < 0) return false;
            if (target.GetTag(BattleTagType.DontAttackedTarget)) return false;
            if ((attacker.transform.pos - target.transform.pos).sqrMagnitude > LMath.Sqr(new LFloat(true, skill.skillConfig.UseDistance))) return false;
            return true;
        }
    }
}