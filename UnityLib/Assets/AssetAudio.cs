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
        public AudioType AudioType { get; private set; } = AudioType.MPEG;

        public AssetAudio(AssetContext context)
            : this(context, AudioType.MPEG) { }

        public AssetAudio(AssetContext context, AudioType audioType) 
            : this(context, audioType, null) 
        {
        }

        public AssetAudio(AssetContext context, AudioType audioType, Action<AssetAudio> onAsync)
            : this(context, onAsync)
        {
            AudioType = audioType;
        }

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
            }, Priority, AudioType);
            return this;
        }
    }
}
