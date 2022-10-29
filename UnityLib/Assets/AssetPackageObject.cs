using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityLib.Loads;
using Object = UnityEngine.Object;

namespace UnityLib.Assets
{
    public class AssetPackageObject : IAssetPackageObject
    {
        public string Uri 
        {
            get => AssetPackage.Uri;
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

        private Object Clone { get; set; }

        public string AssetName { get; private set; }

        public AssetPackage AssetPackage
            => AssetPackageRefrence.AssetPackage;

        public AssetPackageRefrence AssetPackageRefrence { get; set; }

        public AssetPackageObject(string url, Action<AssetPackageObject> onAsync)
            : this(url, string.Empty, false, onAsync) { }

        public AssetPackageObject(string url, bool isMemory,  Action<AssetPackageObject> onAsync)
            : this(url, string.Empty, isMemory, onAsync) { }

        public AssetPackageObject(string url, string assetname, bool isMemory, Action<AssetPackageObject> onAsync)
        {
            AssetName = assetname;
            AssetPackageRefrence = AssetPackageRefrenceUtility
                .GetAssetPackageRefrence(url, isMemory, (package) => { onAsync?.Invoke(this); });
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
            AssetPackageRefrenceUtility.TryDestoryAssetBundleAsync(AssetPackageRefrence);
        }
    }
}
