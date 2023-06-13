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
        /// <summary>
        /// 如果用IoC,那么不要在构造函数中调用该方法
        /// </summary>
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
    }
}
