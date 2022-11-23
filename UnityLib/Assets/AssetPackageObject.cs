using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityLib.Loads;
using Object = UnityEngine.Object;

namespace UnityLib.Assets
{
    public class AssetPackageObject : CustomYieldInstruction, IAssetPackageObject
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

        public AssetPackage AssetPackage
            => AssetPackageRefrence.AssetPackage;

        public AssetPackageRefrence AssetPackageRefrence { get; set; }

        public AssetPackageObject(AssetContext context) : this(context, null) { }

        public AssetPackageObject(AssetContext context, Action<AssetPackageObject> onAsync)
            : this(context, string.Empty, false, onAsync) { }

        public AssetPackageObject(AssetContext context, bool isMemory,  Action<AssetPackageObject> onAsync)
            : this(context, string.Empty, isMemory, onAsync) { }

        public AssetPackageObject(AssetContext context, string assetname, bool isMemory, Action<AssetPackageObject> onAsync)
        {
            AssetName = assetname;
            AssetPackageRefrence = AssetPackageRefrenceUtility
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
            AssetPackageRefrenceUtility.TryDestoryAssetBundleAsync(AssetPackageRefrence);
        }
    }
}
