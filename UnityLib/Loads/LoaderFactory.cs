using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityLib.Loads
{
    public class LoaderFactory
    {
        public static Loader New(string url)
        {
            return new Loader(UnityWebRequest.Get(url));
        }

        public static Loader NewAudio(string url, AudioType audioType)
        {
            return new Loader(UnityWebRequestMultimedia.GetAudioClip(url, audioType));
        }

        public static Loader NewImage(string url)
        {
            return new Loader(UnityWebRequestTexture.GetTexture(url));
        }

        public static Loader NewBundle(string url)
        {
            return new Loader(UnityWebRequestAssetBundle.GetAssetBundle(url));
        }
    }
}
