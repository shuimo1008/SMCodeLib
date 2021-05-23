using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZCSharpLib;
using ZCSharpLib.Events;
using ZCSharpLib.Objects;
using ZCSharpLib.Times;
using ZCSharpLib.Coroutines;
using ZCSharpLib.Cores;

namespace ZGameLib.Assets
{
    public enum LoadPriority
    {
        General = 0, Middle = 1, High = 2,
    }

    /// <summary>
    /// 资源管理
    /// 1. 构建并行加载的数量;
    /// 2. 根据加载优先级加载资源;
    /// 3. 成功加载的资源进入缓存;
    /// </summary>
    public class AssetMgr : ObjectEvent
    {
        private Asset[] assetLoading;
        private Queue<Asset> highAssets;
        private Queue<Asset> middleAssets;
        private Queue<Asset> generalAssets;
        /// <summary>
        /// 资源缓存池
        /// </summary>
        private Dictionary<string, Asset> assetPool;

        public AssetMgr() : this(3) { }

        public AssetMgr(int num)
        {
            assetLoading = new Asset[num];
            highAssets = new Queue<Asset>(); 
            middleAssets = new Queue<Asset>();
            generalAssets = new Queue<Asset>();
            assetPool = new Dictionary<string, Asset>();

            App.SubscribeUpdate(Update);
        }


        void Update(float deltaTime)
        {
            for (int i = 0; i < assetLoading.Length; i++)
            {
                Asset oAsset = assetLoading[i];
                if (oAsset != null)
                {
                    if (oAsset.IsDone)
                    {
                        if (!oAsset.IsSucess)
                        {
                            if (assetPool.ContainsKey(oAsset.Url))
                                assetPool.Remove(oAsset.Url);
                        }
                        assetLoading[i] = null;
                    }

                    // 资源下载状态更新
                    oAsset.Update(deltaTime);
                }
            }

            // 更新工作区
            for (int i = 0; i < assetLoading.Length; i++)
            {
                Asset oAsset = assetLoading[i];
                if (oAsset == null)
                {
                    if (highAssets.Count > 0)
                    {
                        oAsset = highAssets.Dequeue();
                    }
                    else if (middleAssets.Count > 0)
                    {
                        oAsset = middleAssets.Dequeue();
                    }
                    else if (generalAssets.Count > 0)
                    {
                        oAsset = generalAssets.Dequeue();
                    }
                    assetLoading[i] = oAsset;
                    // 资源构建WWW方法
                    if (oAsset != null) oAsset.Start();
                }
            }
        }

        public void Load(string url, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General)
        {
            Asset oAsset = null;
            if (!assetPool.TryGetValue(url, out oAsset))
            {
                oAsset = new Asset(url);
                assetPool.Add(url, oAsset);
            }
            oAsset.AddListener(onDone);
            if (oAsset.IsDone && oAsset.IsSucess)
            {
                oAsset.Callback();
            }
            switch (priority)
            {
                case LoadPriority.General:
                    generalAssets.Enqueue(oAsset);
                    break;
                case LoadPriority.Middle:
                    middleAssets.Enqueue(oAsset);
                    break;
                case LoadPriority.High:
                    highAssets.Enqueue(oAsset);
                    break;
            }
        }

        public void Clear(string url)
        {
            Asset oAsset = null;
            if (assetPool.TryGetValue(url, out oAsset))
            {
                oAsset.Dispose();
                assetPool.Remove(url);
            }
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            App.UnsubscribeUpdate(Update);
        }
    }
}
