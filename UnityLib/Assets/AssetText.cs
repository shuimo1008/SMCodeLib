using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetText : AssetUnity<string, AssetText>
    {
        public AssetText(string uri) : base(uri, null) { }

        public AssetText(string uri, Action<AssetText> onAsync)
            : base(uri, onAsync) { }

        public override string GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetText();
        }

        protected override AssetText StartAsync()
        {
            IoC.Resolve<ILoaderSer>().Load(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
