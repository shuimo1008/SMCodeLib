using SMCore;
using SMCore.Enums;
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

        public ProcessStatus Status
        {
            get
            {
                if (Loader == null)
                    return ProcessStatus.None;
                return Loader.Status;
            }
        }

        protected Loader Loader { get; set; }

        public UnityAsset(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("Uri不能为空");
            Uri = uri;
            UnityAssetRefrenceS.GetRefrence(Uri).Increment();
        }

        public virtual void GetAsync(Action<IUnityAsset<T>> onAsync)
        {

        }

        public abstract T GetAsset();

        public virtual void Dispose()
        {
            // 只有已经加载过才减少引用计数
            int counting = UnityAssetRefrenceS.GetRefrence(Uri).Decrement();
            if (counting == 0)
            {
                IoC.Resolve<ILoadSer>().Unload(Uri);
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

        public override void GetAsync(Action<IUnityAsset<Texture2D>> onAsync)
        {
            base.GetAsync(onAsync);

            IoC.Resolve<ILoadSer>().LoadImage(Uri, (args) => 
            { 
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
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

        public override void GetAsync(Action<IUnityAsset<AudioClip>> onAsync)
        {
            base.GetAsync(onAsync);

            IoC.Resolve<ILoadSer>().LoadAudio(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
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

        public override void GetAsync(Action<IUnityAsset<byte[]>> onAsync)
        {
            base.GetAsync(onAsync);

            IoC.Resolve<ILoadSer>().Load(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
        }
    }

    public class BundleAssetS
    {
        private static Dictionary<string, BundleAsset> Bundles
        {
            get
            {
                if (_Bundles == null)
                    _Bundles = new Dictionary<string, BundleAsset>();
                return _Bundles;
            }
        }
        private static Dictionary<string, BundleAsset> _Bundles;

        public static void GetAsync(string uri, Action<BundleAsset> onAsync)
        {
            if (!Bundles.TryGetValue(uri, out var bundle))
            {
                Bundles.Add(uri, bundle = new BundleAsset(uri));
                bundle.OnWillbeDestoryed = OnDestoryHandler;
                bundle.GetAsync((asset) => { onAsync?.Invoke(asset as BundleAsset); });
            }
            else bundle.GetAsync((asset) => { onAsync?.Invoke(asset as BundleAsset); });
        }

        public static void Destory(string uri) => OnDestoryHandler(uri);

        private static void OnDestoryHandler(string uri)
        {
            if (Bundles.TryGetValue(uri, out var bundle))
                Bundles.Remove(uri);
            if (bundle != null) bundle.Dispose();
        }
    }

    public class BundleAsset : UnityAsset<AssetBundle>
    {
        private bool IsMemory { get; set; }

        public Action<string> OnWillbeDestoryed { get; set; }

        private int _counting;

        //private Dictionary<string, int> Countings
        //{
        //    get
        //    {
        //        if (_Countings == null)
        //            _Countings = new Dictionary<string, int>();
        //        return _Countings;
        //    }
        //}
        //private Dictionary<string, int> _Countings;

        public BundleAsset(string uri)
            : base(uri) { }

        public BundleAsset(string uri, bool isMemory)
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

        public IBundleObject GetAsset(string name)
        {
            if (Loader == null) return null;

            //if (Countings.TryGetValue(name, out var counting))
            //    Countings[name] = ++counting;
            _counting = _counting + 1;

            BundleObject bundleObject = new BundleObject(name);
            bundleObject.OnDispose = OnBundleDestoryHandler;
            bundleObject.Asset = Loader.GetAsset(name, IsMemory);

            return bundleObject;
        }

        private void OnBundleDestoryHandler(string name)
        {
            //if (Countings.TryGetValue(name, out var counting))
            //    Countings[name] = --counting;
            _counting = Mathf.Max(0, _counting - 1);

            if (_counting == 0) OnWillbeDestoryed?.Invoke(Uri);
        }

        public override AssetBundle GetAsset()
        {
            throw new Exception("该方法未实现, 请使用GetAsset(string name)获取详细资源!");
        }

        public override void GetAsync(Action<IUnityAsset<AssetBundle>> onAsync)
        {
            base.GetAsync(onAsync);

            IoC.Resolve<ILoadSer>().LoadBundle(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
        }
    }

    public class BundleObject : IBundleObject
    {
        public string Name { get; }

        public Object Asset { get; set; }

        public Action<string> OnDispose { get; set; }

        public BundleObject(string name) => Name = name;

        private Object clone;

        public T As<T>() where T : Object
        {
            return Asset as T;
        }

        public T AsClone<T>() where T : Object
        {
            clone = Object.Instantiate(Asset as T);
            return clone as T;
        }

        public void Dispose()
        {
            if (clone != null)
                Object.DestroyImmediate(clone);
            OnDispose?.Invoke(Name);
        }
    }
}
