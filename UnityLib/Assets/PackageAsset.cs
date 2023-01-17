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
    public class PackageAsset : UnityAsset<AssetBundle, PackageAsset>
    {
        private bool IsMemory { get; set; }

        public PackageAsset(AssetContext context) : base(context) { }

        public PackageAsset(AssetContext context, Action<PackageAsset> onAsync)
            : this(context, false, onAsync) { }

        public PackageAsset(AssetContext context, bool isMemory, Action<PackageAsset> onAsync)
            : base(context, onAsync) 
        { 
            IsMemory = isMemory;
        }

        protected override PackageAsset StartAsync()
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
