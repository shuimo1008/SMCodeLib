using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.UnityAssets
{
    public interface IUnityAsset<T> : IDisposable
    {
        string Uri { get; }
        string Error { get; }
        bool IsDone { get; }
        bool IsSucess { get; }
        float Progress { get; }
        Priority Priority { get; }

        void GetAsync(Action<IUnityAsset<T>> onAsync);

        T GetAsset();
    }
}
