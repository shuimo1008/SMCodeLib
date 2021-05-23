using System;
using System.Collections.Generic;
using ZCSharpLib.Cores;
using ZCSharpLib.Events;
using ZCSharpLib.Objects;
using ZCSharpLib.Times;

namespace ZCSharpLib.Webs
{
    public class HttpLoadAll : ObjectEvent
    {
        public class HttpPaths
        {
            public string Url { get; set; }
            public string SavePath { get; set; }
        }

        public int LoadIndex { get; private set; }
        public int LoadNum { get; private set; }
        public string[] Urls { get; private set; }
        public string[] SavePaths { get; private set; }
        public float SingleProgress { get; private set; }
        public float FinalProgress { get; private set; }
        public bool IsDone { get; private set; }
        public bool IsStart { get; private set; }
        public string Error { get; private set; }
        private List<HttpLoad> WebLoaders { get; set; }
        private List<HttpLoad> AlreadyLoaders { get; set; }
        private Action<HttpLoadAll> OnAllLoaded { get; set; }
        private Action<HttpLoadAll> OnAllLoading { get; set; }
        private Action<HttpLoadAll> OnError { get; set; }
        private Action<HttpLoad> OnSingleLoaded { get; set; }
        private Action<HttpLoad> OnSingleLoading { get; set; }
        private HttpLoad mCurLoader;
        private float mTotalProgress;

        public HttpLoadAll()
        {
            IsDone = false;
            Error = string.Empty;
            WebLoaders = new List<HttpLoad>();
            AlreadyLoaders = new List<HttpLoad>();
            App.SubscribeUpdate(Update);
        }

        public void SetEventSingleLoaded(Action<HttpLoad> onSingleLoaded)
        {
            OnSingleLoaded = onSingleLoaded;
        }

        public void SetEventSingleLoading(Action<HttpLoad> onSingleLoading)
        {
            OnSingleLoading = onSingleLoading;
        }

        public void SetEventAllLoaded(Action<HttpLoadAll> onAllLoaded)
        {
            OnAllLoaded = onAllLoaded;
        }

        public void SetEventAllLoading(Action<HttpLoadAll> onAllLoading)
        {
            OnAllLoading = onAllLoading;
        }

        public void LoadAll(HttpPaths[] dirs)
        {
            LoadNum = dirs.Length;
            Urls = new string[LoadNum];
            SavePaths = new string[LoadNum];
            for (int i = 0; i < dirs.Length; i++)
            {
                HttpPaths dir = dirs[i];
                Urls[i] = dir.Url;
                SavePaths[i] = dir.SavePath;
                HttpLoad oLoader = new HttpLoad(Urls[i], SavePaths[i]);
                oLoader.SetEventLoading(SingleLoading);
                oLoader.SetEventLoaded(SingleLoaded);
                WebLoaders.Add(oLoader);
            }
        }

        public void UnloadAll()
        {
            WebLoaders.Clear();
            for (int i = 0; i < AlreadyLoaders.Count; i++)
            {
                HttpLoad oLoader = AlreadyLoaders[i];
                if(oLoader.IsStart) oLoader.Close();
                oLoader.Clear();
            }
        }

        public HttpLoad GetLoader(string url, string savePath)
        {
            HttpLoad oLoader = WebLoaders.Find((t)=> { return t.Url == url && t.SavePath == savePath; });
            return oLoader;
        }

        private void SingleLoading(HttpLoad loader)
        {
            try
            {
                OnSingleLoading?.Invoke(loader);
            }
            catch (Exception e)
            {
                App.Error(e);
            }
            AllLoading(loader);
        }

        private void SingleLoaded(HttpLoad loader)
        {
            if (!string.IsNullOrEmpty(loader.Error))
            {
                loader.Close(); // 下载失败则关闭
                Error = Error + "\n" + loader.Error;
            }
            OnSingleLoaded?.Invoke(loader);

            if (LoadIndex == LoadNum) IsDone = true;
            if (IsDone) OnAllLoaded?.Invoke(this);
        }

        private void AllLoading(HttpLoad loader)
        {
            try
            {
                SingleProgress = loader.Progress;
                mTotalProgress = LoadIndex + SingleProgress;
                FinalProgress = mTotalProgress / LoadNum;
                if (loader.IsDone) LoadIndex = LoadIndex + 1;
                OnAllLoading?.Invoke(this);
            }
            catch (Exception e)
            {
                App.Error(e);
            }
            if (IsDone) Close();
        }

        public void Start()
        {
            IsStart = true;
        }

        public void Close()
        {
            IsStart = false;
        }

        public void Update(float deltaTime)
        {
            if (!IsStart) return;

            if (mCurLoader == null)
            {
                if (WebLoaders.Count > 0)
                {
                    mCurLoader = WebLoaders[0];
                    WebLoaders.RemoveAt(0);
                    mCurLoader.Start();
                    AlreadyLoaders.Add(mCurLoader);
                }
            }
            if (mCurLoader != null)
            {
                mCurLoader.Loop(deltaTime);
                if (mCurLoader.IsDone)
                {
                    mCurLoader.Close();
                    mCurLoader = null;
                }
            }
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            if(WebLoaders.Count > 0) WebLoaders.Clear();
            if(AlreadyLoaders.Count > 0) AlreadyLoaders.Clear();
            App.UnsubscribeUpdate(Update);
        }
    }
}