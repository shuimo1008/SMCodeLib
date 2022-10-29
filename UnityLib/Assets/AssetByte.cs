using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetByte : AssetUnity<byte[], AssetByte>
    {
        public AssetByte(string uri, Action<AssetByte> onAsync)
            : base(uri, onAsync) { }

        public override byte[] GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetBytes();
        }

        public override void StartAsync(Action<AssetByte> onAsync)
        {
            IoC.Resolve<ILoaderSer>().Load(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
        }
    }
}
