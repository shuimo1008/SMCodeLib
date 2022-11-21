using SMCore;
using SMCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityLib.Loads;
using Object = UnityEngine.Object;

namespace UnityLib.Assets
{
    public abstract class AssetUnity<T, U> : CustomYieldInstruction, IAssetUnity<T> where U : IAssetUnity<T>
    {
        public string Uri 
        { 
            get; 
            private set; 
        }
        public Priority Priority 
        { 
            get; 
            set; 
        }

        public string Error 
        {
            get
            {
                if (Loader == null) 
                    return string.Empty;
                return Loader.Error;
            }
        }
        public bool IsDone 
        {
            get
            {
                if (Loader == null)
                    return false;
                return Loader.IsDone;
            }
        }
        public bool IsSucess 
        {
            get
            {
                if (Loader == null)
                    return false;
                return Loader.IsSucess;
            }
        }
        public float Progress 
        {
            get
            {
                if (Loader == null)
                    return 0;
                return Loader.Progress;
            }
        }

        public ProcessStatus Status
        {
            get
            {
                if (Loader == null)
                    return ProcessStatus.None;
                return Loader.Status;
            }
        }

        public override bool keepWaiting => !IsDone;

        protected Loader Loader { get; set; }

        protected Action<U> OnAsync { get; set; }

        public AssetUnity(string uri)
            : this(uri, null) { }

        public AssetUnity(string uri, Action<U> onAsync)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("Uri不能为空");
            AssetUnityRefrenceUtility.GetRefrence(uri).Increment();
            Uri = uri;
            OnAsync = onAsync;
            StartAsync();
        }

        public abstract T GetAsset();

        protected abstract U StartAsync();


        public bool IsDisposed { get; protected set; }
        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            // 只有已经加载过才减少引用计数
            int counting = AssetUnityRefrenceUtility.GetRefrence(Uri).Decrement();
            if (counting == 0) IoC.Resolve<ILoaderSer>().Unload(Uri);
        }
    }
}
