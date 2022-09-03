using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.UnityAssets
{
    public interface IUnityAssetRefrence
    {
        int Counting { get;  }

        int Increment();

        int Decrement();
    }
}
