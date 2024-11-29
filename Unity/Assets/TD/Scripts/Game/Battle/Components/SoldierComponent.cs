using System;
using Lockstep.Math;
using TDFramework;

namespace TD
{
    public class SoldierComponent : Component
    {
        public HeroComponent hero;

        public override void Awake()
        {
            throw new System.NotImplementedException();
        }

        public override void Distory()
        {
            if (hero != null)
            {
                hero.RemoveSoldier(this);
                hero = null;
            }
        }

        public override void Update(LFloat delta)
        {
            if (hero != null)
            {
                //跑向英雄
                //英雄有攻击目标 攻击目标
            }
        }

        public void OnHeroDistory()
        {
            hero = null;
        }
    }
}