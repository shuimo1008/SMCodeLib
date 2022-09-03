using SMCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityLib.Loads;
using UnityEngine;

namespace UnityLib.UnityAssets
{
    public class UnityAssetRefrence : IUnityAssetRefrence
    {
        public int Counting
        {
            get => _Counting;
        }
        private int _Counting;

        public int Increment()
        {
            Interlocked.Increment(ref _Counting);
            return _Counting;
        }

        public int Decrement()
        {
            Interlocked.Decrement(ref _Counting);
            return _Counting;
        }
    }
}
