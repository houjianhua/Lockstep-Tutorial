using System;
using System.Collections.Generic;
using System.IO;
using Lockstep.Network;
using UnityEngine;

namespace UnityFramework
{
    public static class ResourceManager
    {
        private static Dictionary<string, UnityEngine.Object> prefabDic = new Dictionary<string, UnityEngine.Object>();

        public static GameObject Load(string path)
        {
            return Load<GameObject>(path);
        }
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            if (prefabDic.TryGetValue(path, out var obj))
            {
                return obj as T;
            }
            else
            {
                obj = Resources.Load<T>(path);
                if (obj != null) prefabDic.Add(path, obj);
                return obj as T;
            }
        }

        public static void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            if (prefabDic.TryGetValue(path, out var obj))
            {
                callback(obj as T);
            }
            else
            {
                var request = Resources.LoadAsync<T>(path);
                request.completed += (operation) =>
                {
                    obj = request.asset;
                    if (!prefabDic.ContainsKey(path)) prefabDic.Add(path, obj);
                    else if (prefabDic[path] != obj)
                    {
                        LLog.Error($"newly loaded asset is different from old cached asset : {path}");
                    }
                    callback(obj as T);
                };
            }
        }



        public static T Instantiate<T>(string dir, Transform parent) where T : UnityEngine.Object
        {
            var prefab = Load<T>(dir);
            if (prefab == null)
            {
                LLog.Error($"not have prefab : {dir}");
                return null;
            }
            var window = UnityEngine.Object.Instantiate<T>(prefab, parent);
            return window;
        }
        public static void InstantiateAsync<T>(string dir, Transform parent, Action<T> callback) where T : UnityEngine.Object
        {
            LoadAsync<T>(dir, (prefab) =>
            {
                if (prefab == null)
                {
                    LLog.Error($"not have prefab : {dir}");
                    callback(null);
                }
                var window = UnityEngine.Object.Instantiate<T>(prefab, parent);
                callback(window);
            });
        }

        public static void ReleaseInstantiate(UnityEngine.Object obj)
        {
            UnityEngine.Object.Destroy(obj);
        }
    }
}