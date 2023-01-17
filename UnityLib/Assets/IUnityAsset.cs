using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;
using UnityEngine;

namespace UnityLib.Assets
{
    public interface IUnityAsset<T> : IDisposable
    {
        AssetContext Context { get; }
        string Error { get; }
        bool IsDone { get; }
        bool IsSucess { get; }
        float Progress { get; }
        Priority Priority { get; }

        T GetAsset();
    }

}
