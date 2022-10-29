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
        public AssetImage(string uri, Action<AssetImage> onAsync)
            : base(uri, onAsync) { }

        public override Texture2D GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetTexture();
        }

        public override void StartAsync(Action<AssetImage> onAsync)
        {
            IoC.Resolve<ILoaderSer>().LoadImage(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
        }
    }
}
