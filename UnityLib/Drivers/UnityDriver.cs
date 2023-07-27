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
        public static UnityDriver Instance
        {
            get
            {
                if (instance == null) 
                {
                    instance = new GameObject("UnityDriver")
                        .AddComponent<UnityDriver>();
                }
                return instance;
            }
        }
        private static UnityDriver instance;

        private Action<float>[] subscribers;

        private Dictionary<int, Action<float>> Subscribers
        {
            get
            {
                if (_Subscribers == null)
                    _Subscribers = new Dictionary<int, Action<float>>();
                return _Subscribers;
            }
        }

        private Dictionary<int, Action<float>> _Subscribers;

        void Awake()
        {
            instance = this;
        }

        void Update() => Update(Time.deltaTime);

        public void Subscribe(Action<float> onDriving)
        {
            int hashID = onDriving.GetHashCode();
            if (!Subscribers.ContainsKey(hashID))
                Subscribers.Add(hashID, onDriving);
            subscribers = Subscribers.Values.ToArray();
        }

        public void Unsubscribe(Action<float> onDriving)
        {
            int hashID = onDriving.GetHashCode();
            if (Subscribers.ContainsKey(hashID))
                Subscribers.Remove(hashID);
            subscribers = Subscribers.Values.ToArray();
        }

        public void Update(float deltaTime)
        {
            if (subscribers == null) return;

            for (int i = 0; i < subscribers.Length; i++)
                subscribers[i].Invoke(deltaTime);
        }
    }
}
