using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDFramework
{
    public class ResLoder : ILife
    {
        private Dictionary<string, List<UnityEngine.Object>> resDic;

        public void Init()
        {
            resDic = new Dictionary<string, List<UnityEngine.Object>>();
        }

        public void Release()
        {
            foreach (var objs in resDic)
            {
                objs.Value.Clear();
            }
            resDic.Clear();
        }

        public void Tick(int delta)
        {
            throw new NotImplementedException();
        }

        public UnityEngine.Object Instantiate(string path)
        {
            var obj = ResManager.Instance.Instantiate(path);
            if (resDic.TryGetValue(path, out var objs))
            {
                objs.Add(obj);
            }
            else
            {
                objs = new List<UnityEngine.Object>() { obj };
                resDic.Add(path, objs);
            }
            return obj;
        }
        public T Instantiate<T>(string path) where T : UnityEngine.Object
        {
            return Instantiate(path) as T;
        }
    }
}