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

namespace ZGameLib.Assets
{
    public class Asset : ObjectEvent, IEventArgs
    {
        public string Url { get; protected set; }
        public bool IsDone { get; protected set; }
        public bool IsSucess { get; protected set; }
        public float Progress { get; protected set; }
        public string Error { get; protected set; }
        private UnityWebRequest www { get; set; }
        private UnityWebRequestAsyncOperation AsyncOperation { get; set; }

        private AssetBundle mAssetBundle;

        private const string ASSETAUDIO = "Audio";
        private const string ASSETIMAGE = "Image";
        private const string ASSETTEXT = "Text";
        private const string ASSETBYTE = "Byte";
        private const string ASSETBUNDLE = "AssetBundle";

        private Dictionary<string, object> CacheTable { get; set; }
        public static Asset MakeGet<T>(string url)
        {
            Asset asset = new Asset();
            asset.Url = url;
            if (typeof(T) == typeof(Texture) ||
                typeof(T) == typeof(Texture2D))
                asset.www = UnityWebRequestTexture.GetTexture(asset.Url);
            else
                asset.www = UnityWebRequest.Get(asset.Url);
            asset.CacheTable = new Dictionary<string, object>();
            return asset;
        }

        private Asset()
        {
        }

        public void Start()
        {
            AsyncOperation = www.SendWebRequest();
        }

        public void Update(float deltaTime)
        {
            bool isDone = www.isDone; // www 是异步操作, 所以这里用变量来记录是否下载完成，防止在同一个方法里面多次使用www.isDone获取的值不同
            IsDone = isDone;
            if (IsDone)
            {
                if (string.IsNullOrEmpty(www.error)) IsSucess = true;
                else { IsSucess = false; Error = www.error; }
            }
            Progress = www.downloadProgress;
            Callback();
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

        private AssetBundle GetBundle()
        {
            if (mAssetBundle == null && www.isDone && string.IsNullOrEmpty(www.error))
            {
                mAssetBundle = DownloadHandlerAssetBundle.GetContent(www);
            }
            return mAssetBundle;
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
                        App.Error("当前资源：{0} AssetBundle中没有找到对应名称 name={1} 的资源!", Url, name);
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
                    Object.DestroyImmediate(obj as Texture2D);
                }
                else if (key.Equals(ASSETAUDIO))
                {
                    Object.DestroyImmediate(obj as AudioClip);
                }
            }
            if (mAssetBundle != null) { mAssetBundle.Unload(true); }
        }
    }
}