using System;
using System.Collections.Generic;
using Lockstep.Logging;
using Lockstep.Math;

namespace TD
{
    public class BattleAttribuite
    {
        public int id;
        private long _value;
        public bool dirty = true;
        public long baseValue;
        public long baseRatio; // 1/1000
        public long baseAdd;
        public long affterRatio; // 1/1000
        public long affterAdd;
        public long value
        {
            get
            {
                if (dirty) Calc();
                return _value;
            }
        }
        private void Calc()
        {
            _value = baseValue + (baseRatio + 1000) / 1000 + baseValue;
            _value = value + (affterRatio + 1000) / 1000 + affterRatio;
        }
    }
    public class BattleAttribuiteManager
    {
        public BattleActor actor;
        private BattleAttributeConfig battleAttributeConfig;
        private Dictionary<AttribuiteType, BattleAttribuite> attribuiteDic;
        public long hp { get; private set; }
        public long mp { get; private set; }
        public void DamageAd(int AdValue)
        {
            var value = AdValue - attribuiteDic[AttribuiteType.AdDef].value;
            if (value < 1) value = 1;
            hp -= value;
        }
        public void DamageAp(int ApValue)
        {
            var value = ApValue - GetAttributeValue(AttribuiteType.ApDef);
            if (value < 1) value = 1;
            hp -= value;
        }
        public void AddHp(int value)
        {
            hp += value;
            var max = GetAttributeValue(AttribuiteType.Hp);
            if (hp > max) hp = max;
        }
        public void Init(int attribuiteId)
        {
            battleAttributeConfig = ConfigManager.Instance.GetConfig<BattleAttributeConfig>(attribuiteId);
            attribuiteDic = new Dictionary<AttribuiteType, BattleAttribuite>();
            InitAttribuite(AttribuiteType.MoveSpeed, battleAttributeConfig.MoveSpeed);
            InitAttribuite(AttribuiteType.Hp, battleAttributeConfig.Hp);
            InitAttribuite(AttribuiteType.Mp, battleAttributeConfig.Mp);
            InitAttribuite(AttribuiteType.Ad, battleAttributeConfig.Ad);
            InitAttribuite(AttribuiteType.AdDef, battleAttributeConfig.AdDef);
            InitAttribuite(AttribuiteType.Ap, battleAttributeConfig.Ap);
            InitAttribuite(AttribuiteType.ApDef, battleAttributeConfig.ApDef);
            hp = GetAttributeValue(AttribuiteType.Hp);
            mp = GetAttributeValue(AttribuiteType.Mp);
        }
        public long GetAttributeValue(AttribuiteType type)
        {
            return attribuiteDic[type].value;
        }
        private void InitAttribuite(AttribuiteType type, long baseValue)
        {
            var attribuite = new BattleAttribuite()
            {
                id = (int)type,
                baseValue = baseValue
            };
            attribuiteDic.Add(type, attribuite);
        }

        public void ChangeAttribuite(AttribuiteType type, AttributeCalcType calcType, long value)
        {
            if (attribuiteDic.TryGetValue(type, out var attribuite))
            {
                switch (calcType)
                {
                    case AttributeCalcType.BaseRatio:
                        attribuite.baseRatio += value;
                        break;
                    case AttributeCalcType.BaseAdd:
                        attribuite.baseAdd += value;
                        break;
                    case AttributeCalcType.AffterRatio:
                        attribuite.affterRatio += value;
                        break;
                    case AttributeCalcType.AffterAdd:
                        attribuite.affterAdd += value;
                        break;
                    default:
                        Debug.LogError("change attribuite error : ", calcType);
                        break;
                }
                attribuite.dirty = true;
            }
            else
            {
                Debug.LogError("change attribuite error : ", type);
            }
        }
    }
}