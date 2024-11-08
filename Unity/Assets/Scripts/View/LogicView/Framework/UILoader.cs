using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Network;
using UnityEngine;

namespace UnityFramework
{
    //每种UI只允许加载1个
    public class UILoader
    {
        private Dictionary<string, UIBase> dir2window = new Dictionary<string, UIBase>();
        public Dictionary<UIBase, string> window2Dir = new Dictionary<UIBase, string>();
        private bool isRelease = false;
        public UIBase GetWindow(string dir)
        {
            if (isRelease) return null;
            this.dir2window.TryGetValue(dir, out var window);
            return window;
        }

        public UIBase LoadWindow(string dir, Transform parent)
        {
            if (isRelease) return null;
            var window = ResourceManager.Instantiate<UIBase>(dir, parent);
            dir2window.Add(dir, window);
            window2Dir.Add(window, dir);
            return window;
        }
        public void LoadWindowAsync(string dir, Transform parent, Action<UIBase> callback)
        {
            if (isRelease)
            {
                callback(null);
                return;
            }
            ResourceManager.InstantiateAsync<UIBase>(dir, parent, (window) =>
            {
                if (isRelease)
                {
                    ResourceManager.ReleaseInstantiate(window.gameObject);
                    callback(null);
                    return;
                }
                dir2window.Add(dir, window);
                window2Dir.Add(window, dir);
                callback(window);
                return;
            });
        }
        public void ReleaseWindow(UIBase ui)
        {
            if (isRelease) return;
            dir2window.Remove(window2Dir[ui]);
            window2Dir.Remove(ui);
            ResourceManager.ReleaseInstantiate(ui.gameObject);
        }
        public void Release()
        {
            isRelease = true;
            dir2window.Clear();
            foreach (var item in window2Dir)
            {
                ResourceManager.ReleaseInstantiate(item.Key.gameObject);
            }
            window2Dir.Clear();
        }
    }

    //每种UI允许加载多个
    public class MultiUILoder
    {
        private Dictionary<string, List<UIBase>> dir2window = new Dictionary<string, List<UIBase>>();
        public Dictionary<UIBase, string> window2Dir = new Dictionary<UIBase, string>();
        private bool isRelease = false;
        public List<UIBase> GetWindows(string dir)
        {
            this.dir2window.TryGetValue(dir, out var windows);
            return windows;
        }

        public UIBase LoadWindow(string dir, Transform parent)
        {
            if (isRelease) return null;
            var window = ResourceManager.Instantiate<UIBase>(dir, parent);
            if (dir2window.TryGetValue(dir, out var windows))
            {
                windows.Add(window);
            }
            else
            {
                dir2window.Add(dir, new List<UIBase>() { window });
            }
            window2Dir.Add(window, dir);
            return window;
        }

        public void LoadWindowAsync(string dir, Transform parent, Action<UIBase> callback)
        {
            if (isRelease)
            {
                callback(null);
                return;
            }

            ResourceManager.InstantiateAsync<UIBase>(dir, parent, (window) =>
            {
                if (isRelease)
                {
                    ResourceManager.ReleaseInstantiate(window.gameObject);
                    callback(null);
                    return;
                }
                if (dir2window.TryGetValue(dir, out var windows))
                {
                    windows.Add(window);
                }
                else
                {
                    dir2window.Add(dir, new List<UIBase>() { window });
                }
                window2Dir.Add(window, dir);
                callback(window);
                return;
            });
        }


        public void ReleaseWindow(UIBase ui)
        {
            if (isRelease) return;
            if (!window2Dir.ContainsKey(ui))
            {
                LLog.Error($"repeat release window");
                return;
            }

            if (dir2window.TryGetValue(window2Dir[ui], out var windows))
            {
                windows.Remove(ui);
            }
            window2Dir.Remove(ui);
            ResourceManager.ReleaseInstantiate(ui.gameObject);
        }

        public void Relice()
        {
            isRelease = true;
            foreach (var item in dir2window)
            {
                item.Value.Clear();
            }
            dir2window.Clear();
            foreach (var item in window2Dir)
            {
                ResourceManager.ReleaseInstantiate(item.Key.gameObject);
            }
            window2Dir.Clear();
        }
    }
}