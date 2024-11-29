using System;
using System.Collections.Generic;
using Lockstep.Collision2D;
using Lockstep.Math;
using TDFramework;

namespace TD
{
    public class BattleScene : ILife
    {
        private List<BattleActor> entities;
        public LevelConfig levelConfig;
        public List<LVector2> monsterPath;
        public EventContainer<SceneEvent> eventContainer;
        public ResLoder resLoder;
        public void Init()
        {
            eventContainer = new EventContainer<SceneEvent>();
            resLoder = new ResLoder();
            resLoder.Init();

            monsterPath = new List<LVector2>() { new LVector2(0, 0), new LVector2(0, 2), new LVector2(1, 3), new LVector2(1, 5) };
            entities = new List<BattleActor>();
        }



        public void Tick(int delta)
        {
            foreach (var obj in entities)
            {
                obj.Tick(delta);
            }
        }

        public void Release()
        {
            ClearObjectAll();
        }

        private void ClearObjectAll()
        {
            foreach (var obj in entities)
            {
                obj.Release();
            }
            entities.Clear();
        }

        public void AddBattleEntity(BattleActor entity)
        {
            entities.Add(entity);
            entity.scene = this;
            entity.Init();
        }

        public BattleActor GetAttackTarget(BattleActor attacker, BattleSkill skill)
        {
            foreach (var entity in entities)
            {
                if (BattleSkill.ChackInUseRange(attacker, entity, skill))
                {
                    return entity;
                }
            }
            return null;
        }
    }
}