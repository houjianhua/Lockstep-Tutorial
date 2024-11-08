using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Lockstep.Network {
    public class LLog {
        public static void Error(string msg){
#if UNITY_5_3_OR_NEWER
            Debug.LogError(msg);
#else
            Console.WriteLine(msg);
#endif
        }

        public static void Log(string msg){
#if UNITY_5_3_OR_NEWER
            Debug.Log(msg);
#else
            Console.WriteLine(msg);
#endif
        }
    }
}