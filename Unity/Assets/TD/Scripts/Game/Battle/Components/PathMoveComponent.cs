using System.Collections.Generic;
using Lockstep.Math;
using TDFramework;

namespace TD
{
    public class PathMoveComponent : Component
    {
        private BattleActor actor => owner as BattleActor;
        public List<LVector2> path;
        private int idx;
        public override void Awake()
        {
            idx = 0;
        }

        public override void Distory()
        {

        }

        public override void Update(LFloat delta)
        {
            if (actor.GetTag(BattleTagType.DontMove)) return;
            if (idx < path.Count)
            {
                var target = path[idx];
                var move = (target - actor.transform.pos).normalized * new LFloat(true, actor.attribuite.GetAttributeValue(AttribuiteType.MoveSpeed)) * delta;
                //移动距离超过目标点
                if (move.sqrMagnitude > (target - actor.transform.pos).sqrMagnitude)
                {
                    actor.transform.pos = target;
                    idx++;
                }
                else
                {
                    actor.transform.pos += move;
                }
            }
            else
            {
                actor.eventContainer.Tigger(ActorEvent.ArrivePathEnd);
            }
        }
    }
}