using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityLib.Loads;
using Object = UnityEngine.Object;

namespace UnityLib.Assets
{
    public class PackageObjectAsset : CustomYieldInstruction, IPackageObjectAsset
    {
        public AssetContext Context 
        {
            get => AssetPackage.Context;
        }
        public string Error 
        {
            get => AssetPackage.Error;
        }

        public bool IsDone
        {
            get => AssetPackage.IsDone;
        }

        public bool IsSucess 
        {
            get => AssetPackage.IsSucess;
        }

        public float Progress
        {
            get => AssetPackage.Progress;
        }

        public Priority Priority 
        {
            get => AssetPackage.Priority;
        }

        public override bool keepWaiting => !IsDone;

        private Object Clone { get; set; }

        public string AssetName { get; private set; }

        public PackageAsset AssetPackage
            => AssetPackageRefrence.AssetPackage;

        public PackageAssetRefrence AssetPackageRefrence { get; set; }

        public PackageObjectAsset(AssetContext context) : this(context, null) { }

        public PackageObjectAsset(AssetContext context, Action<PackageObjectAsset> onAsync)
            : this(context, string.Empty, false, onAsync) { }

        public PackageObjectAsset(AssetContext context, bool isMemory,  Action<PackageObjectAsset> onAsync)
            : this(context, string.Empty, isMemory, onAsync) { }

        public PackageObjectAsset(AssetContext context, string assetname, bool isMemory, Action<PackageObjectAsset> onAsync)
        {
            AssetName = assetname;
            AssetPackageRefrence = PackageAssetRefrenceUtility
                .GetAssetPackageRefrence(context, isMemory, (package) => { onAsync?.Invoke(this); });
            AssetPackageRefrence.Increment(); // 标记引用计数

            if (AssetPackage.IsDone) AssetPackageRefrence.Notify();
        }

        public T As<T>() where T : Object
        {
            if (string.IsNullOrEmpty(AssetName))
            {
                string[] assetNames = AssetPackage.GetAllAssetNames();
                if (assetNames.Length > 0) AssetName = assetNames[0];
            }
            return AssetPackage.GetAsset(AssetName) as T;
        }

        public T AsClone<T>() where T : Object
        {
            T t = As<T>();
            Clone = Object.Instantiate(t);
            return Clone as T;
        }

        protected bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            if (Clone != null)
                Object.DestroyImmediate(Clone);
            AssetPackageRefrence.Decrement();
            PackageAssetRefrenceUtility.TryDestoryAssetBundleAsync(AssetPackageRefrence);
        }
    }
}
