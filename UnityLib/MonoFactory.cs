using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityLib
{
    public class MonoFactory
    {
        public static T Get<T>(GameObject o) where T : MonoBehaviour
        {
            T t = o.GetComponent<T>();
            if (t == null) t = o.AddComponent<T>();
            return t;
        }

        public static T Get<T, U>(GameObject o)
            where U : MonoBehaviour, T => Get<U>(o);
    }
}
