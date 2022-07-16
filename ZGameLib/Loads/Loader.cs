using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine.Networking;
using ZCSharpLib;
using ZCSharpLib.Objects;
using ZCSharpLib.Times;
using ZCSharpLib.Events;
using Object = UnityEngine.Object;
using ZCSharpLib.Cores;

namespace ZGameLib.Loads
{
    public class LoadingFactory
    {
        public static Loader New(string url)
        {
            return new Loader(UnityWebRequest.Get(url));
        }

        public static Loader NewAudio(string url, AudioType audioType)
        {
            return new Loader(UnityWebRequestMultimedia.GetAudioClip(url, audioType));
        }

        public static Loader NewImage(string url)
        {
            return new Loader(UnityWebRequestTexture.GetTexture(url));
        }

        public static Loader NewBundle(string url)
        {
            return new Loader(UnityWebRequestAssetBundle.GetAssetBundle(url));
        }
    }

    public class Loader : ObjectEvent, IEventArgs
    {
        public string Url { get; protected set; }
        public bool IsDone { get; protected set; }
        public bool IsSucess { get; protected set; }
        public float Progress { get; protected set; }
        public string Error { get; protected set; }
        private UnityWebRequest www { get; set; }
        private UnityWebRequestAsyncOperation AsyncOperation { get; set; }

        private AssetBundle assetBundle;

        private const string ASSETAUDIO = "Audio";
        private const string ASSETIMAGE = "Image";
        private const string ASSETTEXT = "Text";
        private const string ASSETBYTE = "Byte";
        private const string ASSETBUNDLE = "AssetBundle";

        private Dictionary<string, object> CacheTable 
        {
            get
            {
                if (_cacheTable == null)
                    _cacheTable = new Dictionary<string, object>();
                return _cacheTable;
            }
        }
        private Dictionary<string, object> _cacheTable;

        public Loader(UnityWebRequest www) 
        {
            this.www = www;
            this.Url = this.www.url;
        }

        public void Start()
        {
            AsyncOperation = www.SendWebRequest();
        }

        public void Update(float deltaTime)
        {
            bool isDone = www.isDone; // www 是异步操作, 所以这里用变量来记录是否下载完成，防止在同一个方法里面多次使用www.isDone获取的值不同
            if (isDone)
            {
                if (string.IsNullOrEmpty(www.error)) IsSucess = true;
                else { IsSucess = false; Error = www.error; }
            }
            Progress = www.downloadProgress;
            IsDone = isDone;
            Callback(); // 回调,发送是否加载完成，是否加载成功，加载进度等等信息
        }

        public void Callback()
        {
            try
            {
                if (!IsDispose)
                    Notify(this);
            }
            catch (Exception e) { App.Info(e); }
            if (IsDone) RemoveAllListener();
        }

        public string[] GetAllScenePaths()
        {
            return GetBundle().GetAllScenePaths();
        }

        public string[] GetAllAssetNames()
        {
            return GetBundle().GetAllAssetNames();
        }

        private bool isResourceUnload = false;
        public void SetResourceUnload(bool isResourceUnload)
        {
            this.isResourceUnload = isResourceUnload;
        }

        private AssetBundle GetBundle()
        {
            if (assetBundle == null && www.isDone && string.IsNullOrEmpty(www.error))
            {
                assetBundle = DownloadHandlerAssetBundle.GetContent(www);
            }
            return assetBundle;
        }

        public Object GetAsset(string name)
        {
            List<Object> list = null;
            if (!CacheTable.ContainsKey(ASSETBUNDLE))
            {
                list = new List<Object>();
                CacheTable.Add(ASSETBUNDLE, list);
            }
            else list = CacheTable[ASSETBUNDLE] as List<Object>;

            AssetBundle bundle = GetBundle();
            Object retObj = null;
            if (bundle != null)
            {
                retObj = list.Find((t) => t.name.Equals(name));
                if (retObj == null)
                {
                    retObj = bundle.LoadAsset(name);
                    if (retObj == null)
                    {
                        App.Error($"当前资源：{Url} AssetBundle中没有找到对应名称 name={name} 的资源!");
                    }
                    else { list.Add(retObj); }
                }
            }
            return retObj;
        }

        public AudioClip GetAudioClip()
        {
            if (!CacheTable.TryGetValue(ASSETAUDIO, out var obj))
            {
                obj = DownloadHandlerAudioClip.GetContent(www);
                CacheTable.Add(ASSETAUDIO, obj);
            }
            return obj as AudioClip;
        }

        public Texture2D GetTexture()
        {
            if (!CacheTable.TryGetValue(ASSETIMAGE, out var obj))
            {
                obj = DownloadHandlerTexture.GetContent(www);
                CacheTable.Add(ASSETIMAGE, obj);
            }
            return obj as Texture2D;
        }

        public string GetText()
        {
            System.Object obj = null;
            if (!CacheTable.TryGetValue(ASSETTEXT, out obj))
            {
                obj = www.downloadHandler.text;
                CacheTable.Add(ASSETTEXT, obj);
            }
            if (obj != null) return obj.ToString();
            else return string.Empty;
        }

        public byte[] GetBytes()
        {
            if (!CacheTable.TryGetValue(ASSETBYTE, out var obj))
            {
                obj = www.downloadHandler.data;
                CacheTable.Add(ASSETBYTE, obj);
            }
            return obj as byte[];
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            www?.Dispose();
        }

        protected override void DoUnManagedObjectDispose()
        {
            base.DoUnManagedObjectDispose();
            List<string> keys = new List<string>(CacheTable.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                object obj = CacheTable[key];
                if (key.Equals(ASSETIMAGE))
                {
                    Texture o = obj as Texture;
                    if (isResourceUnload)
                    {
                        //if (o != null) Resources.UnloadAsset(o);
                    }
                    if (o != null) Object.Destroy(o);
                }
                else if (key.Equals(ASSETAUDIO))
                {
                    AudioClip o = obj as AudioClip;
                    if (isResourceUnload)
                    {
                        //if (o != null) Resources.UnloadAsset(o);
                    }
                    if (o != null) Object.Destroy(o);
                }
            }
            if (assetBundle != null) { assetBundle.Unload(true); }
        }
    }
}