using System;
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
    public class LoaderSer : ObjectBase, ILoaderSer
    {
        private ILoader[] loadingGroup;
        private Queue<ILoader> waitLoading1Queue; // 最高优先级
        private Queue<ILoader> waitLoading2Queue; // 中等优先级
        private Queue<ILoader> waitLoading3Queue; // 最低优先级

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
            Driver.Subscribe(Update);
        }

        void Update(float deltaTime)
        {
            for (int i = 0; i < loadingGroup.Length; i++)
            {
                ILoader loader = loadingGroup[i];

                if (loader != null && loader.IsDisposed)
                {
                    loader = null;
                    loadingGroup[i] = null;
                }

                if (loader != null && loader.IsDone)
                {
                    if (!loader.IsSucess)
                    {
                        if (Caches.ContainsKey(loader.Uri))
                            Caches.Remove(loader.Uri);
                    }
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

        public void Load(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            Load(uri, string.Empty, onDone, priority);
        }

        public void LoadImage(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            LoadImage(uri, string.Empty, onDone, priority);
        }

        public void LoadBundle(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            LoadBundle(uri, string.Empty, onDone, priority);
        }

        public void LoadAudio(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General, AudioType audioType = AudioType.MPEG)
        {
            LoadAudio(uri, string.Empty, onDone, priority, audioType);
        }

        public void Load(string uri, string version, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            Load(uri, (_uri) => { return Factory.New(_uri, version); }, onDone, priority);
        }

        public void LoadImage(string uri, string version, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            Load(uri, (_uri) => { return Factory.NewImage(_uri, version); }, onDone, priority);
        }

        public void LoadBundle(string uri, string version, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            Load(uri, (_uri) => { return Factory.NewBundle(_uri, version); }, onDone, priority);
        }

        public void LoadAudio(string uri, string version, Action<IEventArgs> onDone, Priority priority = Priority.General, AudioType audioType = AudioType.MPEG)
        {
            Load(uri, (_uri) => { return Factory.NewAudio(_uri, version, audioType); }, onDone, priority);
        }

        private void Load(string url, Func<string, ILoader> f, Action<IEventArgs> onDone, Priority priority = Priority.General)
        {
            if (string.IsNullOrEmpty(url))
                throw new Exception("Loader方法 Url 不能为空\n");

            bool needLoad = true;
            if (Caches.TryGetValue(url, out ILoader loader))
            {
                // 资源池已经有该资源，但是资源没有加载成功,则对资源进行重新加载
                if (loader.IsDone && !loader.IsSucess)
                {
                    // 先释放资源,然后再重新加载
                    Unload(url);
                    // 重新加载资源
                    Caches.Add(url, f.Invoke(url));// Asset.New<T>(context);
                }
                else needLoad = false;
            }
            else Caches.Add(url, f.Invoke(url)); // 加入资源池

            if (needLoad)
            {
                // 进入等待加载队列
                switch (priority)
                {
                    case Priority.High:
                        waitLoading1Queue.Enqueue(Caches[url]);
                        break;
                    case Priority.Middle:
                        waitLoading2Queue.Enqueue(Caches[url]);
                        break;
                    case Priority.General:
                        waitLoading3Queue.Enqueue(Caches[url]);
                        break;
                }
            }
            // 事件监听写在这里是因为存在同一时间多个地方需要加载该资源。
            // 所以每个地方都需要在资源完成加载后进行回调，于是当资源完成回调后删除所有监听事件。
            Caches[url].AddListener(onDone);
            // 如果资源已经完成下载,则直接返回
            if (Caches[url].IsDone) Caches[url].Callback();
        }

        public void Unload(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new Exception("Loader方法 Url 不能为空\n");

            ILoader oAsset;
            if (Caches.TryGetValue(uri, out oAsset))
            {
                Caches.Remove(uri);
                oAsset.Dispose();
            }
        }

        public void UnloadAll()
        {
            foreach (var assetCache in Caches.Values)
                assetCache.Dispose();
            Caches.Clear();
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            Driver.Unsubscribe(Update);
        }
    }
}
