using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{

    public interface IAssetPackageObject : IDisposable
    {
        string Uri{ get; }
        string Error { get; }
        bool IsDone { get; }
        bool IsSucess { get; }
        float Progress { get; }
        Priority Priority { get; }
        string AssetName { get; }

        T As<T>() where T : UnityEngine.Object;
        T AsClone<T>() where T : UnityEngine.Object;
    }
}
