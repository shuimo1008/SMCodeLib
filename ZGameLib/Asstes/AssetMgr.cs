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
        private Asset[] loadingGroup;
        private Queue<Asset> waitLoading1Queue; // 最高优先级
        private Queue<Asset> waitLoading2Queue; // 中等优先级
        private Queue<Asset> waitLoading3Queue; // 最低优先级
        /// <summary>
        /// 资源缓存池
        /// </summary>
        private Dictionary<string, Asset> AssetPool { get; set; }

        public AssetMgr() : this(3) { }

        public AssetMgr(int num)
        {
            loadingGroup = new Asset[num];
            waitLoading1Queue = new Queue<Asset>(); 
            waitLoading2Queue = new Queue<Asset>();
            waitLoading3Queue = new Queue<Asset>();
            AssetPool = new Dictionary<string, Asset>();
            App.SubscribeUpdate(Update);
        }


        void Update(float deltaTime)
        {
            for (int i = 0; i < loadingGroup.Length; i++)
            {
                Asset oAsset = loadingGroup[i];
                if (oAsset != null)
                {
                    if (oAsset.IsDone)
                    {
                        if (!oAsset.IsSucess)
                        {
                            if (AssetPool.ContainsKey(oAsset.Url))
                                AssetPool.Remove(oAsset.Url);
                        }
                        loadingGroup[i] = null;
                    }

                    // 资源下载状态更新
                    oAsset.Update(deltaTime);
                }
            }

            // 更新工作区
            for (int i = 0; i < loadingGroup.Length; i++)
            {
                var loading = loadingGroup[i];
                if (loading == null)
                {
                    if (waitLoading1Queue.Count > 0)
                        loading = waitLoading1Queue.Dequeue();
                    else if (waitLoading2Queue.Count > 0)
                        loading = waitLoading2Queue.Dequeue();
                    else if (waitLoading3Queue.Count > 0)
                        loading = waitLoading3Queue.Dequeue();
                    loadingGroup[i] = loading;
                    // 资源构建WWW方法
                    if (loadingGroup[i] != null) loadingGroup[i].Start();
                }
            }
        }

        public void Load<T>(AssetContext context, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General) where T : class
        {
            bool needLoad = true;
            if (AssetPool.TryGetValue(context.url, out Asset oAsset))
            {
                // 资源池已经有该资源，但是资源没有加载成功,则对资源进行重新加载
                if (oAsset.IsDone && !oAsset.IsSucess)
                {
                    AssetPool[context.url].Dispose();
                    AssetPool[context.url] = Asset.New<T>(context);
                }
                else needLoad = false;
            }
            else AssetPool.Add(context.url, Asset.New<T>(context)); // 加入资源池

            if (needLoad)
            {
                // 进入等待加载队列
                switch (priority)
                {
                    case LoadPriority.High:
                        waitLoading1Queue.Enqueue(AssetPool[context.url]);
                        break;
                    case LoadPriority.Middle:
                        waitLoading2Queue.Enqueue(AssetPool[context.url]);
                        break;
                    case LoadPriority.General:
                        waitLoading3Queue.Enqueue(AssetPool[context.url]);
                        break;
                }
            }
            // 事件监听写在这里是因为存在同一时间多个地方需要加载该资源。
            // 所以每个地方都需要在资源完成加载后进行回调，于是当资源完成回调后删除所有监听事件。
            AssetPool[context.url].AddListener(onDone);
            // 如果资源已经完成下载,则直接返回
            if (AssetPool[context.url].IsDone) AssetPool[context.url].Callback();
        }

        public void Clear(string url)
        {
            Asset oAsset;
            if (AssetPool.TryGetValue(url, out oAsset))
            {
                oAsset.Dispose();
                AssetPool.Remove(url);
            }
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            App.UnsubscribeUpdate(Update);
        }
    }
}
