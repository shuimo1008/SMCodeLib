using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMCore;
using SMCore.Driver;
using SMCore.Events;
using SMCore.Objects;
using UnityEngine;

namespace UnityLib.Loads
{
    public enum Priority
    {
        General = 0, Middle = 1, High = 2,
    }

    /// <summary>
    /// 资源管理
    /// 1. 构建并行加载的数量;
    /// 2. 根据加载优先级加载资源;
    /// 3. 成功加载的资源进入缓存;
    /// </summary>
    public class LoaderSer : ObjectEvent<ILoader>, ILoaderSer
    {
        private ILoader[] loadingGroup;
        private Queue<ILoader> waitLoading1Queue; // 最高优先级
        private Queue<ILoader> waitLoading2Queue; // 中等优先级
        private Queue<ILoader> waitLoading3Queue; // 最低优先级
        private ConcurrentDictionary<string, ILoader> toBeloads;

        public IReadOnlyDictionary<string, ILoader> ToBeloads => toBeloads;

        private IDriverSer Driver
        {
            get
            {
                if (_Driver == null)
                    _Driver = IoC.Resolve<IDriverSer>();
                return _Driver;
            }
        }
        private IDriverSer _Driver;

        public ILoaderFactory Factory
        {
            get
            {
                if (_Factory == null)
                    _Factory = new LoaderFactory();
                return _Factory;
            }
        }
        private ILoaderFactory _Factory;

        /// <summary>
        /// 缓存加载者
        /// </summary>
        private Dictionary<string, ILoader> Caches { get; set; }

        public LoaderSer() : this(3) { }

        public LoaderSer(int num)
        {
            loadingGroup = new Loader[num];
            waitLoading1Queue = new Queue<ILoader>(); 
            waitLoading2Queue = new Queue<ILoader>();
            waitLoading3Queue = new Queue<ILoader>();
            Caches = new Dictionary<string, ILoader>();
            toBeloads = new ConcurrentDictionary<string, ILoader>();
            Driver.Subscribe(Update);
        }

        void Update(float deltaTime)
        {
            for (int i = 0; i < loadingGroup.Length; i++)
            {
                ILoader loader = loadingGroup[i];

                if (loader != null && loader.IsDisposed)
                {
                    if (toBeloads.TryRemove(loader.Url, out _)) { };

                    loader = null;
                    loadingGroup[i] = null;
                }

                if (loader != null && loader.IsDone)
                {
                    if (!loader.IsSucess)
                    {
                        if (Caches.ContainsKey(loader.Url))
                            Caches.Remove(loader.Url);
                    }
                    // 加载器完成加载后从待加载队列删除该加载器
                    if (toBeloads.TryRemove(loader.Url, out _)) { }

                    loadingGroup[i] = null;
                }

                if (loader != null && !loader.IsDone)
                    loader.Update(deltaTime);   // 资源下载状态更新
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
                    // 资源构建WWW方法,排除掉已经资源释放的物体
                    if (loadingGroup[i] != null && !loadingGroup[i].IsDisposed)
                        loadingGroup[i].Start();
                }
            }
        }

        public void Load(AssetContext context, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            Load(context, (_uri) => { return Factory.New(context); }, onDone, priority);
        }

        public void LoadImage(AssetContext context, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            Load(context, (_uri) => { return Factory.NewImage(context); }, onDone, priority);
        }

        public void LoadBundle(AssetContext context, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            Load(context, (_uri) => { return Factory.NewBundle(context); }, onDone, priority);
        }

        public void LoadAudio(AssetContext context, Action<IEventArgs> onDone, Priority priority = Priority.General, AudioType audioType = AudioType.MPEG)
        {
            Load(context, (_uri) => { return Factory.NewAudio(context, audioType); }, onDone, priority);
        }

        private void Load(AssetContext context, Func<AssetContext, ILoader> f, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            if(context.Equals(default(AssetContext)))
                throw new Exception("Loader方法 AssetInfo 不能为空\n");

            if (string.IsNullOrEmpty(context.Url))
                throw new Exception("Loader方法 AssetInfo.Url 不能为空\n");

            bool needLoad = true;
            if (Caches.TryGetValue(context.Url, out ILoader loader))
            {
                // 资源池已经有该资源，但是资源没有加载成功,则对资源进行重新加载
                if (loader.IsDone && !loader.IsSucess)
                {
                    // 先释放资源,然后再重新加载
                    Unload(context.Url);

                    // 重新加载资源
                    Caches.Add(context.Url, f.Invoke(context));// Asset.New<T>(context);
                }
                else needLoad = false;
            }
            else
            {
                loader = f.Invoke(context);
                //IoC.Resolve<SMCore.Logger.ILoggerS>().Debug("Regist Url: " + info.Url);
                // 加入资源池
                Caches.Add(context.Url, loader); 
            }

            if (needLoad)
            {
                // 进入等待加载队列
                switch (priority)
                {
                    case Priority.High:
                        waitLoading1Queue.Enqueue(Caches[context.Url]);
                        break;
                    case Priority.Middle:
                        waitLoading2Queue.Enqueue(Caches[context.Url]);
                        break;
                    case Priority.General:
                        waitLoading3Queue.Enqueue(Caches[context.Url]);
                        break;
                }

                // 进入待加载队列, 以便查询
                toBeloads.TryAdd(context.Url, loader);
                Notify(loader); // 通知新的加载进入队列
            }
            // 事件监听写在这里是因为存在同一时间多个地方需要加载该资源。
            // 所以每个地方都需要在资源完成加载后进行回调，于是当资源完成回调后删除所有监听事件。
            Caches[context.Url].AddListener(onDone);
            // 如果资源已经完成下载,则直接返回
            if (Caches[context.Url].IsDone) Caches[context.Url].Callback();
        }

        public void Unload(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new Exception("Loader方法 Url 不能为空\n");

            ILoader loader;
            if (Caches.TryGetValue(uri, out loader))
                Caches.Remove(uri);
            if (loader != null) loader.Dispose();
        }

        public void UnloadAll()
        {
            IList<string> urls = new List<string>(Caches.Keys);
            foreach (var url in urls) Unload(url);
            urls.Clear();   
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            Driver.Unsubscribe(Update);
        }
    }
}
