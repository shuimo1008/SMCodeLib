using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace UnityLib.Assets
{
    public class AssetUnityRefrenceUtility
    {
        public static ConcurrentDictionary<string, IAssetUnityRefrence> Refrences
        {
            get
            {
                if (_Refrences == null)
                    _Refrences = new ConcurrentDictionary<string, IAssetUnityRefrence>();
                return _Refrences;
            }
        }
        private static ConcurrentDictionary<string, IAssetUnityRefrence> _Refrences;

        public static IAssetUnityRefrence GetRefrence(string uri)
        {
            if (!Refrences.TryGetValue(uri, out var value))
                Refrences.TryAdd(uri, value = new AssetUnityRefrence());
            return value;
        }
    }
}
