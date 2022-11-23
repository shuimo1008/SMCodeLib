using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetText : AssetUnity<string, AssetText>
    {
        public AssetText(AssetContext context) : base(context, null) { }

        public AssetText(AssetContext context, Action<AssetText> onAsync)
            : base(context, onAsync) { }

        public override string GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetText();
        }

        protected override AssetText StartAsync()
        {
            IoC.Resolve<ILoaderSer>().Load(Context, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
