using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZCSharpLib.Exceptions;
using ZCSharpLib.Utils;

namespace ZCSharpLib.Times
{
    /// <summary>
    /// 程序更新者
    /// </summary>
    public class Updater
    {
        public bool IsUpdate { get; private set; }
        private List<Action<float>> Listeners { get; set; }

        public Updater()
        {
            Listeners = new List<Action<float>>();
        }

        public void Add(Action<float> listener)
        {
            if (!Listeners.Contains(listener))
                Listeners.Add(listener);
        }

        public void Remove(Action<float> listener)
        {
            if (Listeners.Contains(listener))
                Listeners.Remove(listener);
        }

        public void Update(float deltaTime)
        {
            IsUpdate = true;
            for (int i = 0; i < Listeners.Count; i++)
            {
                Action<float> listener = Listeners[i];
                listener(deltaTime);
            }
        }
    }
}
