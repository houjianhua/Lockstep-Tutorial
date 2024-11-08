using System;
using System.Collections.Generic;
using Lockstep.Game;
using Lockstep.Util;
using UnityEngine;
namespace UnityFramework
{
    public class UIBase : MonoBehaviour, IUI
    {
        public bool isRelease { get; private set; } = false;
        private MultiUILoder loader;
        /// <summary>
        /// 创建子UI
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected UIBase CreateUI(string path, RectTransform parent)
        {
            if (isRelease) return null;
            loader ??= new MultiUILoder();
            var window = loader.LoadWindow(path, parent);
            window.Create();
            window.Open();
            return window;
        }

        /// <summary>
        /// 创建子UI
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected void CreateUIAsync(string path, RectTransform parent, Action<UIBase> callback = null)
        {
            if (isRelease)
            {
                callback?.Invoke(null);
                return;
            }
            loader ??= new MultiUILoder();
            loader.LoadWindowAsync(path, parent, (window) =>
           {
               if (window == null)
               {
                   callback?.Invoke(null);
                   return;
               }
               window.Create();
               window.Open();
               callback?.Invoke(window);
           });
        }

        /// <summary>
        /// 移除子UI
        /// </summary>
        /// <param name="ui"></param>
        protected void ReleaseUI(UIBase ui)
        {
            if (ui.isRelease) return;
            ui.Close();
            ui.Release();
            if (loader != null)
            {
                loader.ReleaseWindow(ui);
            }
        }

        private List<int> delayActions;
        protected int CreateDelayAction(float seconds, Action callback)
        {
            var id = TimeUtil.CreateDelayAction(seconds, callback);
            delayActions ??= new List<int>();
            delayActions.Add(id);
            return id;
        }

        protected void RemoveDelayAction(int id)
        {
            TimeUtil.RemoveDelayAction(id);
            delayActions?.Remove(id);
        }


        public void Create()
        {
            isRelease = false;
            DoCreate();
        }
        public void Open()
        {
            if (isRelease) return;
            gameObject.SetActive(true);
            DoOpen();
        }
        public void Close()
        {
            if (isRelease) return;
            DoClose();
            if (delayActions != null && delayActions.Count > 0)
            {
                foreach (var id in delayActions)
                {
                    TimeUtil.RemoveDelayAction(id);
                }
                delayActions.Clear();
            }
            gameObject.SetActive(false);
        }

        public void Release()
        {
            if (isRelease) return;
            isRelease = true;
            DoRelease();
            if (loader != null)
            {
                foreach (var item in loader.window2Dir)
                {
                    item.Key.Close();
                    item.Key.Release();
                }
                loader.Relice();
            }
        }
        protected virtual void DoCreate()
        {

        }
        protected virtual void DoOpen()
        {

        }
        protected virtual void DoClose()
        {

        }
        protected virtual void DoRelease()
        {

        }
    }
}