using System;
using System.Collections.Generic;
using System.Reflection;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Network;
using UnityEngine;
using UnityFramework;

namespace Lockstep.Game
{
    public class UnityUIService : BaseService, IUIService
    {
        public bool IsDebugMode => false;
        private Dictionary<EWindowLayer, RectTransform> windowParent = new Dictionary<EWindowLayer, RectTransform>();
        private UILoader loader = new UILoader();

        public void OpenWindow(WindowCreateInfo info, UICallback callback = null)
        {
            OpenWindow(info.resDir, info.depth, callback);
        }

        public void CloseWindow(string dir)
        {
            var window = loader.GetWindow(dir);
            if (window != null)
            {
                window.Close();
            }
        }

        public void CloseWindow(IUI window)
        {
            if (window is UIBase)
            {
                (window as UIBase).Close();
            }
        }

        public void OpenWindow(string dir, EWindowLayer depth, UICallback callback = null)
        {
            var window = loader.GetWindow(dir);
            if (window != null)
            {
                window.Open();
                callback?.Invoke(window);
                return;
            }
            window = loader.LoadWindow(dir, windowParent[depth]);
            window.Create();
            window.Open();
            callback?.Invoke(window);
        }



        public void RegisterAssembly(Assembly uiAssembly)
        {

        }

        public override void DoStart()
        {
            var canvas = GameObject.Find("Canvas").transform;
            var layer = ResourceManager.Load<GameObject>("Prefabs/UI/Framework/ui_layer");
            foreach (EWindowLayer windowLayer in Enum.GetValues(typeof(EWindowLayer)))
            {
                var obj = GameObject.Instantiate(layer, canvas);
                obj.name = windowLayer.ToString();
                windowParent.Add(windowLayer, obj.GetComponent<RectTransform>());
            }
        }
    }
}