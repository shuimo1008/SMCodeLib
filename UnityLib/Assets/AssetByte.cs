using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetByte : AssetUnity<byte[], AssetByte>
    {
        public AssetByte(string uri) : base(uri) { }

        public AssetByte(string uri, Action<AssetByte> onAsync)
            : base(uri, onAsync) { }

        public override byte[] GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetBytes();
        }

        protected override AssetByte StartAsync()
        {
            IoC.Resolve<ILoaderSer>().Load(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
