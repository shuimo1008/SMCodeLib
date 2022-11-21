using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetImage : AssetUnity<Texture2D, AssetImage>
    {
        public AssetImage(string uri) : base(uri) { }

        public AssetImage(string uri, Action<AssetImage> onAsync)
            : base(uri, onAsync) { }

        public override Texture2D GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetTexture();
        }

        protected override AssetImage StartAsync()
        {
            IoC.Resolve<ILoaderSer>().LoadImage(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
