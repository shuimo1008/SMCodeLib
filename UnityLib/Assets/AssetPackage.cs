using SMCore;
using SMCore.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityLib.Loads;
using Object = UnityEngine.Object;

namespace UnityLib.Assets
{
    public class AssetPackage : AssetUnity<AssetBundle, AssetPackage>
    {
        private bool IsMemory { get; set; }

        public AssetPackage(string uri, Action<AssetPackage> onAsync)
            : this(uri, false, onAsync) { }

        public AssetPackage(string uri, bool isMemory, Action<AssetPackage> onAsync)
            : base(uri, onAsync) 
        { 
            IsMemory = isMemory;
        }

        public override void StartAsync(Action<AssetPackage> onAsync)
        {
            IoC.Resolve<ILoaderSer>().LoadBundle(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
        }

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

        public Object GetAsset(string assetName)
        {
            return Loader.GetAsset(assetName, IsMemory);
        }

        public override AssetBundle GetAsset() => Loader.GetBundle(IsMemory);
    }
}
