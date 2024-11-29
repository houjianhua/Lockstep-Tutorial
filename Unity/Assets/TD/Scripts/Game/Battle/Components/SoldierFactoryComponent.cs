using System.Collections.Generic;
using Lockstep.Collision2D;
using Lockstep.Math;
using TDFramework;

namespace TD
{
    public class SoldierFactoryComponent : Component
    {
        public BattleActor actor => owner as BattleActor;
        public BattleScene scene => actor.scene;
        private readonly int spawneCd = 2000;
        private readonly int soldierMax = 10;
        private readonly int solderId = 100;
        private List<SoldierComponent> soldiers;
        private int spawneTimer;
        public override void Awake()
        {
            soldiers = new List<SoldierComponent>();
            actor.scene.eventContainer.AddListener<HeroComponent>(SceneEvent.HeroCreated, OnHeroCreated);
            spawneTimer = spawneCd;
        }

        private void OnHeroCreated(HeroComponent hero)
        {
            if (hero.actor.compType == this.actor.compType)
            {
                while (soldiers.Count > 0)
                {
                    var soldier = soldiers[soldiers.Count - 1];
                    var b = hero.AddSoldier(soldier);
                    if (b) soldiers.Remove(soldier);
                    else break;
                }

            }
        }

        public override void Distory()
        {
            soldiers = null;
        }

        public override void Update(LFloat delta)
        {
            if (soldiers.Count < soldierMax)
            {
                spawneTimer -= delta._val;
                if (spawneTimer <= 0)
                {
                    SpawneSoldier();
                    spawneTimer = spawneCd;
                }
            }
        }

        private void SpawneSoldier()
        {
            var actor = new BattleActor();
            actor.compType = this.actor.compType;
            actor.configId = solderId;
            actor.transform = new CTransform2D(this.actor.transform.pos + this.actor.transform.forward * soldiers.Count);
            var soldier = actor.AddComponent<SoldierComponent>();
            soldiers.Add(soldier);
            scene.AddBattleEntity(actor);
        }
    }
}