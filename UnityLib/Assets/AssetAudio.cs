using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetAudio : AssetUnity<AudioClip, AssetAudio>
    {
        public AssetAudio(AssetContext context) : base(context) { }

        public AssetAudio(AssetContext context, Action<AssetAudio> onAsync)
            : base(context, onAsync) { }

        public override AudioClip GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetAudioClip();
        }

        protected override AssetAudio StartAsync()
        {
            IoC.Resolve<ILoaderSer>().LoadAudio(Context, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority);
            return this;
        }
    }
}
