using Lockstep.Math;

namespace TDFramework
{
    public interface IComponent
    {
        public Entity owner { get; set; }
        public bool started { get; set; }
        public bool distoryed { get; set; }
        public void Awake();
        public void Start();
        public void Update(LFloat delta);
        public void Distory();
    }

    public abstract class Component : IComponent
    {
        public Entity owner { get; set; }
        public bool started { get; set; }
        public bool distoryed { get; set; }
        public abstract void Awake();
        public abstract void Distory();
        public virtual void Start() { }
        public abstract void Update(LFloat delta);
    }
}