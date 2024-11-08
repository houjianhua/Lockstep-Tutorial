using System;
using System.Collections.Generic;
using System.Linq;

namespace Lockstep.Util
{
    public static class TimeUtil
    {
        class DelayAction
        {
            public Action callback;
            public float time;
            public int id;
        }
        private static Stack<DelayAction> cahceAction = new Stack<DelayAction>();
        private static Dictionary<int, DelayAction> idx2Action = new Dictionary<int, DelayAction>();
        private static int idx = 0;
        public static int CreateDelayAction(float seconds, Action callback)
        {
            if (callback == null)
            {
                return 0;
            }
            if (seconds <= 0)
            {
                callback();
                return 0;
            }
            var id = ++idx;
            var action = cahceAction.Count > 0 ? cahceAction.Pop() : new DelayAction();
            action.id = id;
            action.callback = callback;
            action.time = seconds;
            idx2Action.Add(id, action);
            return id;
        }
        private static List<int> removeAction = new List<int>();
        public static void RemoveDelayAction(int id)
        {
            if (id <= 0) return;
            if (idx2Action.TryGetValue(id, out var action))
            {
                action.callback = null;
                removeAction.Add(id);
            }
        }

        public static void DoUpdate(float deltaTime)
        {
            if (removeAction.Count > 0)
            {
                foreach (var id in removeAction)
                {
                    if (idx2Action.TryGetValue(id, out var action))
                    {
                        idx2Action.Remove(id);
                        action.callback = null;
                        cahceAction.Push(action);
                    }
                }
                removeAction.Clear();
            }

            if (idx2Action.Count > 0)
            {
                foreach (var action in idx2Action.Values.ToArray())
                {
                    if (action.callback == null) continue;
                    action.time -= deltaTime;
                    if (action.time <= 0)
                    {
                        action.callback();
                        action.callback = null;
                        removeAction.Add(action.id);
                    }
                }
            }
        }
    }
}