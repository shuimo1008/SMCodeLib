using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityLib.Assets
{
    public interface IUnityAsset : IDisposable
    {
        AssetContext Context { get; }
        string Error { get; }
        bool IsDone { get; }
        bool IsSucess { get; }
        float Progress { get; }
        Priority Priority { get; }

        UnityWebRequest GetWebRequest();
    }

    public interface IUnityAsset<T> : IUnityAsset
    {
        T GetAsset();
    }

}
