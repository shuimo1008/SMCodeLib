using SMCore;
using SMCore.Driver;
using SMCore.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityLib.Loggers;

namespace UnityLib.Drivers
{
    public class UnityDriver : MonoBehaviour
    {
        private static UnityDriver ins;

        public IDriverService Service
        {
            get
            {
                if (_Service == null)
                {
                    _Service = IoC.Resolve<IDriverService>();
                }
                return _Service;
            }
        }
        private IDriverService _Service;

        private void Awake()
        {
            IoC.Register<IDriverService>(new DriverService());
            ins = this;
        }

        void Update() => Service.Update(Time.deltaTime);

        public static UnityDriver Get()
        {
            if (ins == null)
            {
                GameObject o = new GameObject("UnityDriver");
                DontDestroyOnLoad(o);
                ins = o.AddComponent<UnityDriver>();
            }
            return ins;
        }
    }
}
