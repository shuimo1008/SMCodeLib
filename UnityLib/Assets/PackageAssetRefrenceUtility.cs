using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class PackageAssetRefrenceUtility
    {
        private static Dictionary<string, PackageAssetRefrence> Refrences
        {
            get
            {
                if (_Refrences == null)
                    _Refrences = new Dictionary<string, PackageAssetRefrence>();
                return _Refrences;
            }
        }
        private static Dictionary<string, PackageAssetRefrence> _Refrences;

        public static PackageAssetRefrence GetAssetPackageRefrence(AssetContext context, bool isMemory, Action<PackageAsset> onAsync)
        {
            if (!Refrences.TryGetValue(context.Url, out var refrence))
            {
                Refrences.Add(context.Url, refrence = new PackageAssetRefrence(context, isMemory));
            }
            refrence.AddListener(onAsync);
            return refrence;
        }

        public static void TryDestoryAssetBundleAsync(PackageAssetRefrence refrence)
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
