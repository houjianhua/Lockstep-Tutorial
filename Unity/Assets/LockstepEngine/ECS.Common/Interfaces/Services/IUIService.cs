using System;
using System.Reflection;

namespace Lockstep.Game
{
    public enum EWindowLayer
    {
        Normal,
        Notice,
        Forward,
    }

    public struct WindowCreateInfo
    {
        public string resDir;
        public EWindowLayer depth;

        public WindowCreateInfo(string dir, EWindowLayer dep)
        {
            this.resDir = dir;
            this.depth = dep;
        }
    }

    public delegate void UICallback(object windowObj);

    public interface IUIService : IService
    {
        void RegisterAssembly(Assembly uiAssembly);
        void OpenWindow(string dir, EWindowLayer dep, UICallback callback = null);
        void CloseWindow(string dir);
        void CloseWindow(IUI window);
    }
}