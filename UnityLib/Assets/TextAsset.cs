using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class TextAsset : UnityAsset<string, TextAsset>
    {
        public TextAsset(AssetContext context) : base(context, null) { }

        public TextAsset(AssetContext context, Action<TextAsset> onAsync)
            : base(context, onAsync) { }

        public override string GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetText();
        }

        protected override TextAsset StartAsync()
        {
            IoC.Resolve<ILoaderSer>().Load(Context, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
