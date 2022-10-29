using System;
using System.Collections.Generic;
using System.Text;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public interface IAssetUnityRefrence
    {
        int RefrenceCounting { get;  }

        int Increment();

        int Decrement();
    }
}
