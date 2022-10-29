using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityLib.Loads
{
    public interface ILoaderFactory
    {
        ILoader New(string url);
        ILoader NewAudio(string url, AudioType audioType);
        ILoader NewImage(string url);
        ILoader NewBundle(string url);
        ILoader New(string url, string version);
        ILoader NewAudio(string url, string version, AudioType audioType);
        ILoader NewImage(string url, string version);
        ILoader NewBundle(string url, string version);
    }

    public class LoaderFactory : ILoaderFactory
    {
        public ILoader New(string url)
        {
            return New(url, LoaderContext.DefaultVersion);
        }

        public ILoader NewAudio(string url, AudioType audioType)
        {
            return NewAudio(url, string.Empty, audioType);
        }

        public ILoader NewImage(string url)
        {
            return NewImage(url, string.Empty);
        }

        public ILoader NewBundle(string url)
        {
            return NewBundle(url, string.Empty);
        }

        public ILoader New(string url, string version)
        {
            string requestUrl = GetUrl(url, version);
            return new Loader(UnityWebRequest.Get(requestUrl), version);
        }

        public ILoader NewAudio(string url, string version, AudioType audioType)
        {
            string requestUrl = GetUrl(url, version);
            return new Loader(UnityWebRequestMultimedia.GetAudioClip(requestUrl, audioType), version);
        }

        public ILoader NewImage(string url, string version)
        {
            string requestUrl = GetUrl(url, version);
            return new Loader(UnityWebRequestTexture.GetTexture(requestUrl), version);
        }

        public ILoader NewBundle(string url, string version)
        {
            string requestUrl = GetUrl(url, version);
            return new Loader(UnityWebRequestAssetBundle.GetAssetBundle(requestUrl), version);
        }

        private string GetUrl(string url, string version)
        {
            string requestUrl = url;
            if (!string.IsNullOrEmpty(version))
                requestUrl = $"{requestUrl}?version={version}";
            return requestUrl;
        }
    }
}
