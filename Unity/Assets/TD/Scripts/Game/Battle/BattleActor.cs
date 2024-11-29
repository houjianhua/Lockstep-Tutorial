using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Lockstep.Collision2D;
using Lockstep.Logging;
using Lockstep.Math;
using TDFramework;
namespace TD
{
    //战斗角色
    public class BattleActor : Entity, ILife
    {
        public IBattleActorView view;
        public BattleScene scene;
        public BattleType compType;
        public int configId;
        public BattleActorConfig actorConfig => ConfigManager.Instance.GetConfig<BattleActorConfig>(configId);
        public CTransform2D transform;
        public BattleBuffManager buffMgr;
        public BattleAttribuiteManager attribuite;
        public EventContainer<ActorEvent> eventContainer;

        private Dictionary<BattleTagType, Func<bool>> tagDic;

        public void AddTag(BattleTagType type, Func<bool> tag)
        {
            if (tagDic.TryGetValue(type, out var tags))
            {
                tagDic[type] = tags + tag;
            }
            else
            {
                tagDic[type] = tag;
            }
        }
        public void RemoveTag(BattleTagType type, Func<bool> tag)
        {
            tagDic[type] -= tag;
        }

        public bool GetTag(BattleTagType type)
        {
            return tagDic.TryGetValue(type, out var tags) && tags();
        }

        public override void Init()
        {
            base.Init();
            eventContainer = new EventContainer<ActorEvent>();
            attribuite = new BattleAttribuiteManager();
            attribuite.actor = this;
            attribuite.Init(actorConfig.Attribute);
            buffMgr = new BattleBuffManager();
            buffMgr.actor = this;
            buffMgr.Init();
            tagDic = new Dictionary<BattleTagType, Func<bool>>();
        }

        public override void Release()
        {
            tagDic.Clear();
            base.Release();
        }

        public override void Tick(int delta)
        {
            base.Tick(delta);
        }

        public void Damage(BattleActor battleActor, BattleSkill battleSkill, BattleSkillEffect battleSkillEffect)
        {
            switch ((SkillEffectType)battleSkillEffect.effectConfig.EffectType)
            {
                case SkillEffectType.Ad:
                    attribuite.DamageAd(battleSkillEffect.effectConfig.EffectValue);
                    break;
                case SkillEffectType.Ap:
                    attribuite.DamageAp(battleSkillEffect.effectConfig.EffectValue);
                    break;
                case SkillEffectType.Buff:
                    buffMgr.AddBuff(battleActor, battleSkill, battleSkillEffect);
                    break;
                default:
                    Debug.LogWarning("other skill effect type : ", battleSkillEffect.effectConfig.EffectType.ToString());
                    break;
            }
        }
    }
}