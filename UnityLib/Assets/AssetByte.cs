using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetByte : AssetUnity<byte[], AssetByte>
    {
        public AssetByte(AssetContext context) : base(context) { }

        public AssetByte(AssetContext context, Action<AssetByte> onAsync)
            : base(context, onAsync) { }

        public override byte[] GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetBytes();
        }

        protected override AssetByte StartAsync()
        {
            IoC.Resolve<ILoaderSer>().Load(Context, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
