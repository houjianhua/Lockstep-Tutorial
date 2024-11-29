using System;
using System.Collections.Generic;
using Lockstep.Logging;

namespace TDFramework
{
    public class EventContainer<E>
    {
        private Dictionary<E, Delegate> eventDic = new Dictionary<E, Delegate>();

        public void AddListener(E id, Action callback)
        {
            if (eventDic.ContainsKey(id))
            {
                var old = eventDic[id] as Action;
                if (old == null)
                {
                    Debug.LogError("add listener error : id = ", id);
                    return;
                }
                eventDic[id] = old + callback;
            }
            else
            {
                eventDic[id] = callback;
            }
        }

        public void AddListener<T>(E id, Action<T> callback)
        {
            if (eventDic.ContainsKey(id))
            {
                var old = eventDic[id] as Action<T>;
                if (old == null)
                {
                    Debug.LogError("add listener error : id = ", id);
                    return;
                }
                eventDic[id] = old + callback;
            }
            else
            {
                eventDic[id] = callback;
            }
        }

        public void AddListener<T, G>(E id, Action<T, G> callback)
        {
            if (eventDic.ContainsKey(id))
            {
                var old = eventDic[id] as Action<T, G>;
                if (old == null)
                {
                    Debug.LogError("add listener error : id = ", id);
                    return;
                }
                eventDic[id] = old + callback;
            }
            else
            {
                eventDic[id] = callback;
            }
        }

        public void AddListener<T, G, K>(E id, Action<T, G, K> callback)
        {
            if (eventDic.ContainsKey(id))
            {
                var old = eventDic[id] as Action<T, G, K>;
                if (old == null)
                {
                    Debug.LogError("add listener error : id = ", id);
                    return;
                }
                eventDic[id] = old + callback;
            }
            else
            {
                eventDic[id] = callback;
            }
        }

        public void RemoveListener(E id, Action callback)
        {
            if (eventDic.ContainsKey(id))
            {
                var old = eventDic[id] as Action;
                if (old == null)
                {
                    Debug.LogError("remove listener error : id = ", id);
                    return;
                }
                eventDic[id] = old - callback;
            }
        }
        public void RemoveListener<T>(E id, Action<T> callback)
        {
            if (eventDic.ContainsKey(id))
            {
                var old = eventDic[id] as Action<T>;
                if (old == null)
                {
                    Debug.LogError("remove listener error : id = ", id);
                    return;
                }
                eventDic[id] = old - callback;
            }
        }

        public void RemoveListener<T, G>(E id, Action<T, G> callback)
        {
            if (eventDic.ContainsKey(id))
            {
                var old = eventDic[id] as Action<T, G>;
                if (old == null)
                {
                    Debug.LogError("remove listener error : id = ", id);
                    return;
                }
                eventDic[id] = old - callback;
            }
        }

        public void RemoveListener<T, G, K>(E id, Action<T, G, K> callback)
        {
            if (eventDic.ContainsKey(id))
            {
                var old = eventDic[id] as Action<T, G, K>;
                if (old == null)
                {
                    Debug.LogError("remove listener error : id = ", id);
                    return;
                }
                eventDic[id] = old - callback;
            }
        }

        public void Tigger(E id)
        {
            if (eventDic.TryGetValue(id, out var e))
            {
                var old = e as Action;
                if (old == null)
                {
                    Debug.LogError("tigger error : id = ", id);
                    return;
                }
                old();
            }
        }
        public void Tigger<T>(E id, T t)
        {
            if (eventDic.TryGetValue(id, out var e))
            {
                var old = e as Action<T>;
                if (old == null)
                {
                    Debug.LogError("tigger error : id = ", id);
                    return;
                }
                old(t);
            }
        }
        public void Tigger<T, G>(E id, T t, G g)
        {
            if (eventDic.TryGetValue(id, out var e))
            {
                var old = e as Action<T, G>;
                if (old == null)
                {
                    Debug.LogError("tigger error : id = ", id);
                    return;
                }
                old(t, g);
            }
        }
        public void Tigger<T, G, K>(E id, T t, G g, K k)
        {
            if (eventDic.TryGetValue(id, out var e))
            {
                var old = e as Action<T, G, K>;
                if (old == null)
                {
                    Debug.LogError("tigger error : id = ", id);
                    return;
                }
                old(t, g, k);
            }
        }
    }
}