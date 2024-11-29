using UnityEngine;

namespace TDFramework
{
    public class ResManager
    {
        public static ResManager Instance = new ResManager();
        public UnityEngine.Object Instantiate(string path)
        {
            return UnityEngine.Object.Instantiate(Resources.Load(path));
        }

    }

}
