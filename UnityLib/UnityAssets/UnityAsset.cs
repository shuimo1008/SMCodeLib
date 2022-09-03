using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityLib.Loads;
using Object = UnityEngine.Object;

namespace UnityLib.UnityAssets
{
    public abstract class UnityAsset<T> : IUnityAsset<T>
    {
        public string Uri 
        { 
            get; 
            private set; 
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
        protected Loader Loader { get; set; }

        public UnityAsset(string uri)
        {
            Uri = uri;
            UnityAssetRefrenceS.GetRefrence(Uri).Increment();
        }

        public virtual void GetAsync(Action<IUnityAsset<T>> onAsync)
        {
            IoC.Resolve<ILoaderS>().LoadImage(Uri, (args) => 
            { 
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); } 
            }, Priority);
        }

        public abstract T GetAsset();

        public virtual void Dispose()
        {
            // 只有已经加载过才减少引用计数
            int counting = UnityAssetRefrenceS.GetRefrence(Uri).Decrement();
            if (counting == 0)
            {
                IoC.Resolve<ILoaderS>().Unload(Uri);
            }
        }
    }

    public class ImageAsset : UnityAsset<Texture2D>
    {
        public ImageAsset(string uri)
            : base(uri) { }

        public override Texture2D GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetTexture();
        }
    }

    public class TextAsset : UnityAsset<string>
    {
        public TextAsset(string uri)
            : base(uri) { }

        public override string GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetText();
        }
    }

    public class AudioAsset : UnityAsset<AudioClip>
    {
        public AudioAsset(string uri)
            : base(uri){ }

        public override AudioClip GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetAudioClip();
        }
    }

    public class ByteAsset : UnityAsset<byte[]>
    {
        public ByteAsset(string uri)
            : base(uri) { }

        public override byte[] GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetBytes();
        }
    }

    public class AssetBundleAsset : UnityAsset<AssetBundle>
    {
        private bool IsMemory { get; set; }

        private Dictionary<string, int> Countings
        {
            get
            {
                if (_Countings == null)
                    _Countings = new Dictionary<string, int>();
                return _Countings;
            }
        }
        private Dictionary<string, int> _Countings;

        public AssetBundleAsset(string uri)
            : base(uri) { }

        public AssetBundleAsset(string uri, bool isMemory)
            : base(uri) { IsMemory = isMemory; }

        public string[] GetAllScenePaths()
        {
            if (Loader == null) return null;
            return Loader.GetAllScenePaths(IsMemory);
        }

        public string[] GetAllAssetNames()
        {
            if (Loader == null) return null;
            return Loader.GetAllAssetNames(IsMemory);
        }

        public BundleObject<T> GetAsset<T>(string name)
        {
            if (Loader == null) return null;

            if (Countings.TryGetValue(name, out var counting))
                Countings[name] = ++counting;

            BundleObject<T> bundleObject = new BundleObject<T>(name);
            bundleObject.OnDispose = OnBundleDestoryHandler;

            return  Loader.GetAsset(name, IsMemory);
        }

        private void OnBundleDestoryHandler(string name)
        {
            if (Countings.TryGetValue(name, out var counting))
                Countings[name] = --counting;

            for (int i = 0; i < C; i++)
            {

            }
        }

        public override AssetBundle GetAsset()
        {
            throw new Exception("该方法未实现, 请使用GetAsset(string name)获取详细资源!");
        }
    }

    public class BundleObject<T> : IDisposable
    {
        public string Name { get; }

        public Action<string> OnDispose { get; set; }

        public BundleObject(string name)
        {
            Name = name;
        }

        public void Dispose()
        {
            OnDispose?.Invoke(Name);
        }
    }

}
