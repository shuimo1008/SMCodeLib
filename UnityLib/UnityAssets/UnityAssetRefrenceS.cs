using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace UnityLib.UnityAssets
{
    public class UnityAssetRefrenceS
    {
        public static ConcurrentDictionary<string, IUnityAssetRefrence> Refrences
        {
            get
            {
                if (_Refrences == null)
                    _Refrences = new ConcurrentDictionary<string, IUnityAssetRefrence>();
                return _Refrences;
            }
        }
        private static ConcurrentDictionary<string, IUnityAssetRefrence> _Refrences;

        public static IUnityAssetRefrence GetRefrence(string uri)
        {
            if (!Refrences.TryGetValue(uri, out var value))
                Refrences.TryAdd(uri, value = new UnityAssetRefrence());
            return value;
        }
    }
}
