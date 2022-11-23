using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

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

        public static AssetPackageRefrence GetAssetPackageRefrence(AssetContext context, bool isMemory, Action<AssetPackage> onAsync)
        {
            if (!Refrences.TryGetValue(context.Url, out var refrence))
            {
                Refrences.Add(context.Url, refrence = new AssetPackageRefrence(context, isMemory));
            }
            refrence.AddListener(onAsync);
            return refrence;
        }

        public static void TryDestoryAssetBundleAsync(AssetPackageRefrence refrence)
        {
            if (refrence == null) return;

            if (refrence.RefrenceCounting == 0) 
                Destory(refrence.Context.Url);
        }

        public static void Destory(string url)
        {
            if (Refrences.TryGetValue(url, out var refrence))
                Refrences.Remove(url);
            if (refrence != null) refrence.Dispose();
        }
    }
}
