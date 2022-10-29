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
        public AssetAudio(string uri, Action<AssetAudio> onAsync)
            : base(uri, onAsync) { }

        public override AudioClip GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetAudioClip();
        }

        public override void StartAsync(Action<AssetAudio> onAsync)
        {
            IoC.Resolve<ILoaderSer>().LoadAudio(Uri, (args) =>
            {
                if (args is Loader loader) { Loader = loader; onAsync?.Invoke(this); }
            }, Priority);
        }
    }
}
