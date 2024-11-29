namespace TD
{
    //技能效果
    public class BattleSkillEffect
    {
        public SKillEffectConfig effectConfig;
        private bool isHit;

        public BattleSkillEffect(int id)
        {
            effectConfig = ConfigManager.Instance.GetConfig<SKillEffectConfig>(id);
            isHit = false;
        }

        public void Reset()
        {
            isHit = false;
        }
        public bool Trigger(int time, BattleSkill skill)
        {
            if (isHit) return false;
            if (time >= effectConfig.Trigger)
            {
                isHit = true;
                return true;
            }
            return false;
        }
    }
}