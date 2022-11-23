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

        public AssetPackage(AssetContext context) : base(context) { }

        public AssetPackage(AssetContext context, Action<AssetPackage> onAsync)
            : this(context, false, onAsync) { }

        public AssetPackage(AssetContext context, bool isMemory, Action<AssetPackage> onAsync)
            : base(context, onAsync) 
        { 
            IsMemory = isMemory;
        }

        protected override AssetPackage StartAsync()
        {
            IoC.Resolve<ILoaderSer>().LoadBundle(Context, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
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
