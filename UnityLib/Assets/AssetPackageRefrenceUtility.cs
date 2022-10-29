using System;
using System.Collections.Generic;
using System.Text;

namespace UnityLib.Assets
{
    public class AssetPackageRefrenceUtility
    {
        private static Dictionary<string, AssetPackageRefrence> Refrences
        {
            get
            {
                if (_Refrences == null)
                    _Refrences = new Dictionary<string, AssetPackageRefrence>();
                return _Refrences;
            }
        }
        private static Dictionary<string, AssetPackageRefrence> _Refrences;

        public static AssetPackageRefrence GetAssetPackageRefrence(string uri, bool isMemory, Action<AssetPackage> onAsync)
        {
            if (!Refrences.TryGetValue(uri, out var refrence))
            {
                Refrences.Add(uri, refrence = new AssetPackageRefrence(uri, isMemory));
            }
            refrence.AddListener(onAsync);
            return refrence;
        }

        public static void TryDestoryAssetBundleAsync(AssetPackageRefrence refrence)
        {
            if (refrence == null) return;

            if (refrence.RefrenceCounting == 0) 
                Destory(refrence.Uri);
        }

        public static void Destory(string uri)
        {
            if (Refrences.TryGetValue(uri, out var refrence))
                Refrences.Remove(uri);
            if (refrence != null) refrence.Dispose();
        }
    }
}
