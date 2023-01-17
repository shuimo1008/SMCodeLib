using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class ImageAsset : UnityAsset<Texture2D, ImageAsset>
    {
        public ImageAsset(AssetContext context) : base(context) { }

        public ImageAsset(AssetContext context, Action<ImageAsset> onAsync)
            : base(context, onAsync) { }

        public override Texture2D GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetTexture();
        }

        protected override ImageAsset StartAsync()
        {
            IoC.Resolve<ILoaderSer>().LoadImage(Context, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
