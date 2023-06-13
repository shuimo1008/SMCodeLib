using SMCore;
using SMCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityLib.Loads;
using Object = UnityEngine.Object;

namespace UnityLib.Assets
{
    public abstract class UnityAsset<T, U> : CustomYieldInstruction, IUnityAsset<T> where U : IUnityAsset<T>
    {
        public AssetContext Context 
        { 
            get; 
            protected set; 
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

        public UnityAsset(AssetContext info)
            : this(info, null) { }

        public UnityAsset(AssetContext info, Action<U> onAsync)
        {
            if(info.Equals(default(AssetContext)))
                throw new ArgumentNullException("AssetInfo不能为空");
            if (string.IsNullOrEmpty(info.Url))
                throw new ArgumentNullException("AssetInfo.Url不能为空");

            UnityAssetRefrenceUtility.GetRefrence(info.Url).Increment();
            Context = info;
            OnAsync = onAsync;
            StartAsync();
        }

        public abstract T GetAsset();

        public virtual UnityWebRequest GetWebRequest()
            => Loader.GetWebRequest();

        protected abstract U StartAsync();


        public bool IsDisposed { get; protected set; }
        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            // 只有已经加载过才减少引用计数
            int counting = UnityAssetRefrenceUtility.GetRefrence(Context.Url).Decrement();
            if (counting == 0) IoC.Resolve<ILoaderSer>().Unload(Context.Url);
        }
    }
}
