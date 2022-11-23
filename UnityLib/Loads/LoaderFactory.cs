using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityLib.Loads
{
    public interface ILoaderFactory
    {
        ILoader New(AssetContext context);
        ILoader NewAudio(AssetContext context, AudioType audioType);
        ILoader NewImage(AssetContext context);
        ILoader NewBundle(AssetContext context);
    }

    public class LoaderFactory : ILoaderFactory
    {
        public ILoader New(AssetContext context)
        {
            string requestUrl = GetUrl(context);
            return new Loader(UnityWebRequest.Get(requestUrl), context);
        }

        public ILoader NewAudio(AssetContext context, AudioType audioType)
        {
            string requestUrl = GetUrl(context);
            return new Loader(UnityWebRequestMultimedia.GetAudioClip(requestUrl, audioType), context);
        }

        public ILoader NewImage(AssetContext context)
        {
            string requestUrl = GetUrl(context);
            return new Loader(UnityWebRequestTexture.GetTexture(requestUrl), context);
        }

        public ILoader NewBundle(AssetContext context)
        {
            string requestUrl = GetUrl(context);
            return new Loader(UnityWebRequestAssetBundle.GetAssetBundle(requestUrl), context);
        }

        private string GetUrl(AssetContext context)
        {
            string requestUrl = context.Url;
            if (!string.IsNullOrEmpty(context.Version))
                requestUrl = $"{requestUrl}?version={context.Version}";
            return requestUrl;
        }
    }
}
