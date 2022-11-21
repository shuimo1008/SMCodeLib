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
    public class UnityDriver : MonoBehaviour, IUnityDriver
    {
        private static UnityDriver ins;

        public IDriverSer Service
        {
            get => _Service;
        }
        private IDriverSer _Service;

        private bool isInitialized;

        void Awake()
        {
            ins = this;
            Initalize();
        }

        public void Initalize()
        {
            if (isInitialized) return;
            _Service = IoC.MakeService<IDriverSer>(()=> new DriverSer());
            DontDestroyOnLoad(gameObject);
            isInitialized = true;
        }

        void Update() => Update(Time.deltaTime);

        public void Subscribe(Action<float> onDriving)
        {
            if (!isInitialized) return;
            Service.Subscribe(onDriving);
        }

        public void Unsubscribe(Action<float> onDriving)
        {
            if (!isInitialized) return;
            Service.Unsubscribe(onDriving);
        }

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

        public void Update(float deltaTime)
        {
            if (!isInitialized) return;
            Service.Update(Time.deltaTime);
        }
    }
}
