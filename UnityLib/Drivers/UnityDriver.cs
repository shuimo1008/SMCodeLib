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
    [DefaultExecutionOrder(-1000)]
    public class UnityDriver : MonoBehaviour
    {
        private static UnityDriver ins;

        public IDriverS Service
        {
            get
            {
                if (_Service == null)
                {
                    IoC.Register<IDriverS>(new DriverS());
                    _Service = IoC.Resolve<IDriverS>();
                }
                return _Service;
            }
        }
        private IDriverS _Service;

        private bool isInitialized;

        void Awake()
        {
            ins = this;

            Initalize();
        }

        void Initalize()
        {
            if (!isInitialized) return;
            DontDestroyOnLoad(gameObject);
            isInitialized = true;
        }

        void Update() => Service.Update(Time.deltaTime);

        public void Subscribe(Action<float> onDriving) => Service.Subscribe(onDriving);

        public void Unsubscribe(Action<float> onDriving) => Service.Unsubscribe(onDriving);

        public static UnityDriver Get()
        {
            if (ins == null)
            {
                ins = new GameObject("UnityDriver")
                    .AddComponent<UnityDriver>();
                ins.Initalize();
            }
            return ins;
        }
    }
}
