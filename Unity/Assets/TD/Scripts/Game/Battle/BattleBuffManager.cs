using System;
using System.Collections.Generic;
using TDFramework;

namespace TD
{
    public class BattleBuffManager : ILife
    {
        public BattleActor actor;
        private List<BattleBuff> buffs;
        //public Dictionary<BuffType, BattleBuffEffect> effects;

        public void Init()
        {
            buffs = new List<BattleBuff>();
            //effects = new Dictionary<BuffType, BattleBuffEffect>();
        }

        public void Release()
        {
            foreach (var buff in buffs)
            {
                buff.Release();
            }
            buffs.Clear();
            buffs = null;
            // foreach (var effect in effects)
            // {
            //     effect.Value.Release();
            // }
            // effects.Clear();
            // effects = null;
        }

        public void Tick(int delta)
        {
            foreach (var buff in buffs)
            {
                buff.Tick(delta);
            }
        }

        public void AddBuff(BattleActor battleActor, BattleSkill battleSkill, BattleSkillEffect battleSkillEffect)
        {
            var configId = battleSkillEffect.effectConfig.EffectValue;
            var buffConfig = ConfigManager.Instance.GetConfig<BuffConfig>(configId);
            var buffType = (BuffEffectType)buffConfig.Type;
            // if (!effects.TryGetValue(buffType, out var effect))
            // {
            //     effect = new BattleBuffEffect();
            //     effect.buffType = buffType;
            //     effect.actor = actor;
            //     effects.Add(buffType, effect);
            // }
            var buff = new BattleBuff();
            buff.buffConfig = buffConfig;
            //buff.effect = effect;
            //effect.AddBuff(buff);
            buff.Init();
        }
    }
}