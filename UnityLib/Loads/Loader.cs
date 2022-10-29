using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using SMCore;
using SMCore.Objects;
using SMCore.Events;
using Object = UnityEngine.Object;
using SMCore.Logger;
using SMCore.Enums;

namespace UnityLib.Loads
{
    public class Loader : ObjectEvent, IEventArgs, ILoader
    {
        public string Uri { get; protected set; }
        public string Version { get; protected set; }
        public bool IsDone { get; protected set; }
        public bool IsSucess { get; protected set; }
        public float Progress { get; protected set; }
        public string Error { get; protected set; }
        public ProcessStatus Status { get; protected set; } = ProcessStatus.Prepare;

        private UnityWebRequest www { get; set; }
        private UnityWebRequestAsyncOperation AsyncOperation { get; set; }

        private AssetBundle assetBundle;

        private const string ASSETAUDIO = "Audio";
        private const string ASSETIMAGE = "Image";
        private const string ASSETTEXT = "Text";
        private const string ASSETBYTE = "Byte";
        private const string ASSETBUNDLE = "AssetBundle";

        private ILoggerS LogS
        {
            get
            {
                if (_LogS == null)
                    _LogS = IoC.Resolve<ILoggerS>();
                return _LogS;
            }
        }
        private ILoggerS _LogS;

        private Dictionary<string, object> Cache
        {
            get
            {
                if (_Cache == null)
                    _Cache = new Dictionary<string, object>();
                return _Cache;
            }
        }
        private Dictionary<string, object> _Cache;

        public Loader(UnityWebRequest www) 
            :this(www, string.Empty)
        {
        }

        public Loader(UnityWebRequest www, string version)
        {
            this.www = www;
            Uri = this.www.url;
            Version = version;
            Status = ProcessStatus.Prepare;
            Callback();
        }

        public void Start()
        {
            Status = ProcessStatus.Start;
            Callback();
            AsyncOperation = www.SendWebRequest();
        }

        public void Update(float deltaTime)
        {
            bool isDone = www.isDone; // www 是异步操作, 所以这里用变量来记录是否下载完成，防止在同一个方法里面多次使用www.isDone获取的值不同
            if (isDone)
            {
                Status = ProcessStatus.Finish;
                if (string.IsNullOrEmpty(www.error)) IsSucess = true;
                else { IsSucess = false; Error = www.error + "\n" + Uri; }
            }
            else Status = ProcessStatus.Execute;

            Progress = www.downloadProgress;
            IsDone = isDone;
            Callback(); // 回调,发送是否加载完成，是否加载成功，加载进度等等信息
        }

        public void Callback()
        {
            try
            {
                if (!IsDisposed)
                    Notify(this);
            }
            catch (Exception e) { LogS.Info(e); }
            if (IsDone) RemoveAllListener();
        }

        public string[] GetAllScenePaths(bool fromMemory = false)
        {
            return GetBundle(fromMemory).GetAllScenePaths();
        }

        public string[] GetAllAssetNames(bool fromMemory = false)
        {
            return GetBundle(fromMemory).GetAllAssetNames();
        }

        public AssetBundle GetBundle(bool fromMemory = false)
        {
            if (fromMemory)
            {
                if (assetBundle == null)
                {
                    byte[] bytes = GetBytes();
                    assetBundle = AssetBundle.LoadFromMemory(bytes);
                }
            }
            else
            {
                bool verified = true;
                verified = verified && www.isDone;
                verified = verified && assetBundle == null;
                verified = verified && string.IsNullOrEmpty(Error);
                if (verified) assetBundle = DownloadHandlerAssetBundle.GetContent(www);
            }

            return assetBundle;
        }

        public Object GetAsset(string name, bool fromMemory = false)
        {
            Dictionary<string, Object> bundleAssets;
            if (!Cache.ContainsKey(ASSETBUNDLE))
            {
                bundleAssets = new Dictionary<string, Object>();
                Cache.Add(ASSETBUNDLE, bundleAssets);
            }
            else bundleAssets = Cache[ASSETBUNDLE] as Dictionary<string, Object>;
           
            Object retObj = null;

            AssetBundle bundle = GetBundle(fromMemory);
            if (bundle != null)
            {
                if (!bundleAssets.TryGetValue(name, out retObj))
                {
                    retObj = bundle.LoadAsset(name);
                    if (retObj == null)
                    {
                        LogS.Error($"当前资源：{Uri} AssetBundle中没有找到对应名称 name={name} 的资源!");
                    }
                    else { bundleAssets.Add(name, retObj); }
                }
            }
            return retObj;
        }

        public AudioClip GetAudioClip()
        {
            if (!Cache.TryGetValue(ASSETAUDIO, out var obj))
            {
                obj = DownloadHandlerAudioClip.GetContent(www);
                Cache.Add(ASSETAUDIO, obj);
            }
            return obj as AudioClip;
        }

        public Texture2D GetTexture()
        {
            if (!Cache.TryGetValue(ASSETIMAGE, out var obj))
            {
                obj = DownloadHandlerTexture.GetContent(www);
                Cache.Add(ASSETIMAGE, obj);
            }
            return obj as Texture2D;
        }

        public string GetText()
        {
            object obj;
            if (!Cache.TryGetValue(ASSETTEXT, out obj))
            {
                obj = www.downloadHandler.text;
                Cache.Add(ASSETTEXT, obj);
            }
            if (obj != null) return obj.ToString();
            else return string.Empty;
        }

        public byte[] GetBytes()
        {
            if (!Cache.TryGetValue(ASSETBYTE, out var obj))
            {
                obj = www.downloadHandler.data;
                Cache.Add(ASSETBYTE, obj);
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
            List<string> keys = new List<string>(Cache.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                if (key.Equals(ASSETIMAGE))
                {
                    Texture o = Cache[key] as Texture;
                    if (o != null) Object.Destroy(o);
                }
                else if (key.Equals(ASSETAUDIO))
                {
                    AudioClip o = Cache[key] as AudioClip;
                    if (o != null) Object.Destroy(o);
                }
                else if (key.Equals(ASSETBUNDLE))
                {
                    Dictionary<string, Object> os = Cache[ASSETBUNDLE] as Dictionary<string, Object>;
                    foreach (var o in os.Values)
                    {
                        if (o != null) Object.DestroyImmediate(o, true);
                    }
                    os.Clear();
                }
                Cache[key] = null;
            }
            if (assetBundle != null) { assetBundle.Unload(true); }
            Cache.Clear(); // 缓存清理
        }
    }
}