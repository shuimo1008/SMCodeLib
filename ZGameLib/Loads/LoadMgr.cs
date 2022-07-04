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
using UnityEngine;

namespace ZGameLib.Loads
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
    public class LoadMgr : ObjectEvent
    {
        private Loader[] loadingGroup;
        private Queue<Loader> waitLoading1Queue; // 最高优先级
        private Queue<Loader> waitLoading2Queue; // 中等优先级
        private Queue<Loader> waitLoading3Queue; // 最低优先级
        /// <summary>
        /// 资源缓存池
        /// </summary>
        private Dictionary<string, Loader> AssetPool { get; set; }

        public LoadMgr() : this(3) { }

        public LoadMgr(int num)
        {
            loadingGroup = new Loader[num];
            waitLoading1Queue = new Queue<Loader>(); 
            waitLoading2Queue = new Queue<Loader>();
            waitLoading3Queue = new Queue<Loader>();
            AssetPool = new Dictionary<string, Loader>();
            App.SubscribeUpdate(Update);
        }

        void Update(float deltaTime)
        {
            for (int i = 0; i < loadingGroup.Length; i++)
            {
                Loader oAsset = loadingGroup[i];
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

        public void Load(string url, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General)
        {
            Load(url, (_url)=> { return LoadingFactory.New(_url); }, onDone, priority);
        }

        public void LoadAudio(string url, AudioType audioType, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General)
        {
            Load(url, (_url)=> { return LoadingFactory.NewAudio(_url, audioType); }, onDone, priority);
        }

        public void LoadImage(string url, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General)
        {
            Load(url, (_url) => { return LoadingFactory.NewImage(_url); }, onDone, priority);
        }

        public void LoadBundle(string url, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General)
        {
            Load(url, (_url) => { return LoadingFactory.NewBundle(_url); }, onDone, priority);
        }

        private void Load(string url, Func<string, Loader> f, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General)
        {
            bool needLoad = true;
            if (AssetPool.TryGetValue(url, out Loader oAsset))
            {
                // 资源池已经有该资源，但是资源没有加载成功,则对资源进行重新加载
                if (oAsset.IsDone && !oAsset.IsSucess)
                {
                    AssetPool[url].Dispose();
                    AssetPool[url] = f.Invoke(url);// Asset.New<T>(context);
                }
                else needLoad = false;
            }
            else AssetPool.Add(url, f.Invoke(url)); // 加入资源池

            if (needLoad)
            {
                // 进入等待加载队列
                switch (priority)
                {
                    case LoadPriority.High:
                        waitLoading1Queue.Enqueue(AssetPool[url]);
                        break;
                    case LoadPriority.Middle:
                        waitLoading2Queue.Enqueue(AssetPool[url]);
                        break;
                    case LoadPriority.General:
                        waitLoading3Queue.Enqueue(AssetPool[url]);
                        break;
                }
            }
            // 事件监听写在这里是因为存在同一时间多个地方需要加载该资源。
            // 所以每个地方都需要在资源完成加载后进行回调，于是当资源完成回调后删除所有监听事件。
            AssetPool[url].AddListener(onDone);
            // 如果资源已经完成下载,则直接返回
            if (AssetPool[url].IsDone) AssetPool[url].Callback();
        }

        public void Unload(string url)
        {
            Loader oAsset;
            if (AssetPool.TryGetValue(url, out oAsset))
            {
                AssetPool.Remove(url);
                oAsset.Dispose();
            }
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            App.UnsubscribeUpdate(Update);
        }
    }
}
