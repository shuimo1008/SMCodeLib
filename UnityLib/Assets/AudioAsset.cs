using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AudioAsset : UnityAsset<AudioClip, AudioAsset>
    {
        public AudioType AudioType { get; private set; } = AudioType.MPEG;

        public AudioAsset(AssetContext context)
            : this(context, AudioType.MPEG) { }

        public AudioAsset(AssetContext context, AudioType audioType) 
            : this(context, audioType, null) 
        {
        }

        public AudioAsset(AssetContext context, AudioType audioType, Action<AudioAsset> onAsync)
            : this(context, onAsync)
        {
            AudioType = audioType;
        }

        public AudioAsset(AssetContext context, Action<AudioAsset> onAsync)
            : base(context, onAsync) { }

        public override AudioClip GetAsset()
        {
            if (Loader == null)
                return null;
            return Loader.GetAudioClip();
        }

        protected override AudioAsset StartAsync()
        {
            IoC.Resolve<ILoaderSer>().LoadAudio(Context, (args) =>
            {
                if (args is Loader loader) { Loader = loader; OnAsync?.Invoke(this); }
            }, Priority, AudioType);
            return this;
        }
    }
}
