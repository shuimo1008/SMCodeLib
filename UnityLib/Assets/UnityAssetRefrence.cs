using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityLib.Loads;
using UnityEngine;

namespace UnityLib.Assets
{
    public class UnityAssetRefrence : IUnityAssetRefrence
    {
        public int RefrenceCounting
        {
            get => _RefrenceCounting;
        }
        private int _RefrenceCounting;

        public int Increment()
        {
            Interlocked.Increment(ref _RefrenceCounting);
            return _RefrenceCounting;
        }

        public int Decrement()
        {
            Interlocked.Decrement(ref _RefrenceCounting);
            return _RefrenceCounting;
        }
    }
}
