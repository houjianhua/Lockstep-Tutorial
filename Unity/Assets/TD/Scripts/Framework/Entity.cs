// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Lockstep.Logging;

// namespace TDFramework
// {
//     public abstract class Component
//     {
//         public Component owner { get; private set; }
//         protected abstract void Awake();

//         protected abstract void Update(int delta);
//         protected abstract void Distory();

//         public virtual void Tick(int delta)
//         {
//             if (subComponentDic != null)
//             {
//                 foreach (var component in subComponentDic.Values.ToArray())
//                 {
//                     component.Tick(delta);
//                 }
//             }
//             Update(delta);
//         }
//         private Dictionary<Type, Component> subComponentDic;
//         public bool AddComponent(Component component)
//         {
//             if (subComponentDic == null) subComponentDic = new Dictionary<Type, Component>();
//             var t = component.GetType();
//             if (subComponentDic.ContainsKey(t))
//             {
//                 Debug.LogWarning("repeat add component :", t.Name);
//                 return false;
//             }
//             subComponentDic.Add(t, component);
//             component.owner = this;
//             component.Awake();
//             return true;
//         }
//         public T AddComponent<T>() where T : Component, new()
//         {
//             if (subComponentDic == null) subComponentDic = new Dictionary<Type, Component>();
//             var t = typeof(T);
//             if (subComponentDic.ContainsKey(t))
//             {
//                 Debug.LogWarning("repeat add component :", t.Name);
//                 return null;
//             }
//             var component = new T();
//             subComponentDic.Add(t, component);
//             component.owner = this;
//             component.Awake();
//             return component;
//         }
//         public bool RemoveComponent(Component component)
//         {
//             var b = subComponentDic.Remove(component.GetType());
//             component.Distory();
//             component.owner = null;
//             return b;
//         }
//         public bool RemoveComponent<T>() where T : Component
//         {
//             var b = subComponentDic.TryGetValue(typeof(T), out var component);
//             if (b)
//             {
//                 component.Distory();
//                 component.owner = null;
//             }
//             return b;
//         }
//     }
// }


using System;
using System.Collections.Generic;
using Lockstep.Logging;

namespace TDFramework
{
    public class Entity : ILife
    {
        public virtual void Init()
        {
        }
        public virtual void Release()
        {
            if (addComponents != null && addComponents.Count > 0)
            {
                if (components == null) components = new List<IComponent>();
                foreach (var component in addComponents)
                {
                    components.Add(component);
                }
                addComponents = null;
            }
            if (removeComponents != null && removeComponents.Count > 0)
            {
                foreach (var component in removeComponents)
                {
                    components.Remove(component);
                }
                removeComponents = null;
            }
            if (components != null)
            {
                foreach (var component in components)
                {
                    if (component.distoryed) continue;
                    component.distoryed = true;
                    component.Distory();
                    component.owner = null;
                }
                components = null;
            }
            componentTypes = null;
            if (addComponents != null || removeComponents != null)
            {
                Debug.LogError("bug...");
            }
        }
        public virtual void Tick(int delta)
        {
            if (addComponents != null && addComponents.Count > 0)
            {
                if (components == null) components = new List<IComponent>();
                foreach (var component in addComponents)
                {
                    components.Add(component);
                }
                addComponents.Clear();
            }
            if (removeComponents != null && removeComponents.Count > 0)
            {
                foreach (var component in removeComponents)
                {
                    components.Remove(component);
                }
                removeComponents.Clear();
            }
            if (components != null)
            {
                foreach (var component in components)
                {
                    if (component.started) component.Update(delta);
                    else
                    {
                        component.started = true;
                        component.Start();
                    }
                }
            }
        }
        private List<IComponent> addComponents;
        private List<IComponent> removeComponents;
        private List<IComponent> components;
        private Dictionary<Type, IComponent> componentTypes;//所有组件 没有已删除的
        public bool AddComponent(IComponent component)
        {
            if (componentTypes == null) componentTypes = new Dictionary<Type, IComponent>();
            var t = component.GetType();
            if (componentTypes.ContainsKey(t))
            {
                Debug.LogError("repeat add component");
                return false;
            }
            componentTypes.Add(t, component);
            component.owner = this;
            component.Awake();
            if (addComponents == null) addComponents = new List<IComponent>();
            addComponents.Add(component);
            return true;
        }

        public T AddComponent<T>() where T : IComponent, new()
        {
            if (componentTypes == null) componentTypes = new Dictionary<Type, IComponent>();
            var t = typeof(T);
            if (componentTypes.ContainsKey(t))
            {
                Debug.LogError("repeat add component");
                return (T)componentTypes[t];
            }
            var component = new T();
            if (addComponents == null) addComponents = new List<IComponent>();
            addComponents.Add(component);
            componentTypes.Add(t, component);

            component.owner = this;
            component.Awake();
            return component;
        }
        public void RemoveComponent(IComponent component)
        {
            if (component.distoryed) return;
            if (removeComponents == null) removeComponents = new List<IComponent>();
            removeComponents.Add(component);
            componentTypes.Remove(component.GetType());

            component.distoryed = true;
            component.Distory();
            component.owner = null;
        }
        public void RemoveComponent<T>() where T : IComponent
        {
            if (componentTypes.TryGetValue(typeof(T), out var component))
            {
                RemoveComponent(component);
            }
        }
    }
}