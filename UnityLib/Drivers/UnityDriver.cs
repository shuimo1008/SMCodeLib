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

        public IDriverS Service
        {
            get
            {
                if (_Service == null)
                {
                    _Service = IoC.Resolve<IDriverS>();
                }
                return _Service;
            }
        }
        private IDriverS _Service;

        void Awake()
        {
            IoC.Register<IDriverS>(new DriverS());
            ins = this;
        }

        void Update() => Service.Update(Time.deltaTime);

        public void Subscribe(Action<float> onDriving) => Service.Subscribe(onDriving);

        public void Unsubscribe(Action<float> onDriving) => Service.Unsubscribe(onDriving);

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
