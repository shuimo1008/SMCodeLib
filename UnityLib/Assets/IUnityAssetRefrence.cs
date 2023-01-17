using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public interface IUnityAssetRefrence
    {
        int RefrenceCounting { get;  }

        int Increment();

        int Decrement();
    }
}
