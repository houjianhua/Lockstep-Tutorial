using System.Collections.Generic;
using Lockstep.Math;
using TDFramework;

namespace TD
{
    public class SkillComponent : Component, IBattleSkillHandler
    {
        private List<BattleSkill> skills;
        public BattleActor actor => owner as BattleActor;
        private BattleActor attackTarget;
        public BattleSkill curSkill;
        public override void Awake()
        {
            skills = new List<BattleSkill>();
            foreach (var id in actor.actorConfig.Skills)
            {
                var skill = new BattleSkill();
                skill.configId = id;
                skill.handler = this;
                skill.Init();
                skills.Add(skill);
            }
        }

        public override void Distory()
        {
            foreach (var skill in skills)
            {
                skill.Release();
            }
            skills.Clear();
        }

        public override void Update(LFloat delta)
        {
            foreach (var skill in skills)
            {
                skill.Tick(delta);
            }
            if (curSkill != null && !curSkill.isSkilling && !curSkill.CanUse(actor))
            {
                curSkill = SelectCanUseSkill();
            }
            if (curSkill != null && !actor.GetTag(BattleTagType.DontUseSkill))
            {
                GetAttackTarget();
                if (attackTarget != null)
                {
                    StartUseSkill();
                }
            }
        }
        //筛选优先技能
        private BattleSkill SelectCanUseSkill()
        {
            foreach (var skill in skills)
            {
                if (skill.CanUse(actor) && (curSkill == null || skill.skillConfig.UseSort < curSkill.skillConfig.UseSort))
                {
                    return skill;
                }
            }
            return null;
        }

        private void GetAttackTarget()
        {
            if (attackTarget != null && BattleSkill.ChackInUseRange(actor, attackTarget, curSkill)) return;
            attackTarget = actor.scene.GetAttackTarget(actor, curSkill);
        }

        //开始使用技能
        private void StartUseSkill()
        {
            curSkill.StartUse();
            actor.view.StarUseSkill();
        }

        public void OnSkillTriggerHit(BattleSkill battleSkill, BattleSkillEffect battleSkillEffect)
        {
            attackTarget.Damage(this.actor, battleSkill, battleSkillEffect);
        }
    }
}