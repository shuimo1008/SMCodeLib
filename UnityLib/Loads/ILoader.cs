using SMCore.Events;
using SMCore.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace UnityLib.Loads
{
    public interface ILoader : IEventArgs, IObjectEvent<ILoader>
    {
        string Url { get; }
        bool IsDone { get;  }
        bool IsSucess { get;  }
        float Progress { get;  }
        string Error { get;  }
        AssetContext Context { get; }

        void Start();
        void Update(float deltaTime);
        void Callback();
        UnityWebRequest GetWebRequest();

        /// <summary>获取AssetBundle场景的路径</summary>
        /// <param name="fromMemory">有些情况下无法从AssetBundle中获取资源,需要从内存中获取</param>
        string[] GetAllScenePaths(bool fromMemory = false);
        /// <summary>获取AssetBundle资源的名称</summary>
        /// <param name="fromMemory">有些情况下无法从AssetBundle中获取资源,需要从内存中获取</param>
        string[] GetAllAssetNames(bool fromMemory = false);
        /// <summary>获取AssetBundle中的资源</summary>
        /// <param name="fromMemory">有些情况下无法从AssetBundle中获取资源,需要从内存中获取</param>
        Object GetAsset(string name, bool fromMemory = false);
        AssetBundle GetBundle(bool fromMemory = false);
        AudioClip GetAudioClip();
        Texture2D GetTexture(bool compress = false, bool updateMipmap = false);
        string GetText();
        byte[] GetBytes();
    }
}
