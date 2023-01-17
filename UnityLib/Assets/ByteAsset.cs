using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class ByteAsset : UnityAsset<byte[], ByteAsset>
    {
        public ByteAsset(AssetContext context) : base(context) { }

        public ByteAsset(AssetContext context, Action<ByteAsset> onAsync)
            : base(context, onAsync) { }

        public override byte[] GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetBytes();
        }

        protected override ByteAsset StartAsync()
        {
            IoC.Resolve<ILoaderSer>().Load(Context, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
