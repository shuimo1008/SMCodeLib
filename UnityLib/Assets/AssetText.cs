using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetText : AssetUnity<string, AssetText>
    {
        public AssetText(string uri, Action<AssetText> onAsync)
            : base(uri, onAsync) { }

        public override string GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetText();
        }

        public override void StartAsync(Action<AssetText> onAsync)
        {
            IoC.Resolve<ILoaderSer>().Load(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
        }
    }
}
