using SMCore.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityLib.Drivers
{
    public interface IUnityDriver : IDriverSer
    {
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
    }
}
