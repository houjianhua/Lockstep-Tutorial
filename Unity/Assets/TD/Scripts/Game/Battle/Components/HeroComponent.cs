using System;
using System.Collections.Generic;
using Lockstep.Math;
using TDFramework;

namespace TD
{
    public class HeroComponent : Component
    {
        public BattleActor actor => owner as BattleActor;
        public int solderCountMax = 5;
        private List<SoldierComponent> soldiers;
        public override void Awake()
        {
            soldiers = new List<SoldierComponent>();
            actor.scene.eventContainer.Tigger(SceneEvent.HeroCreated, this);
        }

        public override void Distory()
        {
            if (soldiers != null && soldiers.Count > 0)
            {
                foreach (var soldier in soldiers)
                {
                    soldier.OnHeroDistory();
                }
            }
            soldiers.Clear();
        }

        public override void Update(LFloat delta)
        {
            throw new System.NotImplementedException();
        }

        public bool AddSoldier(SoldierComponent soldier)
        {
            if (soldiers.Count >= solderCountMax) return false;
            soldier.hero = this;
            soldiers.Add(soldier);
            return true;
        }

        public void RemoveSoldier(SoldierComponent soldier)
        {
            soldiers.Remove(soldier);
        }
    }
}