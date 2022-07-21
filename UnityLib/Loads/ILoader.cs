using SMCore.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLib.Loads
{
    public interface ILoader : IObjectEvent
    {
        string Uri { get; }
        string Error { get; }
        bool IsDone { get; }
        bool IsSucess { get; }

        void Start();
        void Update(float deltaTime);
        void Callback();
        string[] GetAllScenePaths();
        string[] GetAllAssetNames();
        Object GetAsset(string name);
        AudioClip GetAudioClip();
        Texture2D GetTexture();
        string GetText();
        byte[] GetBytes();
    }
}
