using SMCore.Driver;
using SMCore.Logger;
using System;
using System.Collections.Generic;

namespace SMCore.Events
{
    public class Event<T> : IEvent<T> where T : IEventArgs
    {
        private ILoggerSer Logger
        {
            get
            {
                if (_Logger == null)
                    _Logger = IoC.Resolve<ILoggerSer>();
                return _Logger;
            }
        }
        private ILoggerSer _Logger;

        protected Dictionary<string, List<Action<T>>> EventDict { get; set; }

        public Event()
        {
            EventDict = new Dictionary<string, List<Action<T>>>();
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听者</param>
        public void AddListener(string eventName, Action<T> listener)
        {
            //App.Logger.Info("参数1：" + eventName);
            if (listener == null) return;
            List<Action<T>> listeners;
            if (!EventDict.TryGetValue(eventName, out listeners))
            {
                listeners = new List<Action<T>>();
                EventDict.Add(eventName, listeners);
            }
            if (!listeners.Contains(listener)) listeners.Add(listener);
        }

        /// <summary>
        /// 移除指定的事件
        /// </summary>
        /// <param name="eventName">指定的事件名</param>
        /// <param name="listener">指定的事件</param>
        public void RemoveListener(string eventName, Action<T> listener)
        {
            if (listener == null) return;
            List<Action<T>> listeners = null;
            if (EventDict.TryGetValue(eventName, out listeners))
            {
                if (listeners.Count > 0)
                {
                    listeners.Remove(listener);
                }
                if (listeners.Count == 0)
                {
                    listeners = null;
                    EventDict.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// 移除列表中所有给定名称的事件
        /// </summary>
        /// <param name="eventName">指定事件的名称</param>
        public void RemoveAllListener(string eventName)
        {
            List<Action<T>> listeners = null;
            if (EventDict.TryGetValue(eventName, out listeners))
            {
                listeners.Clear();
                listeners = null;
                EventDict.Remove(eventName);
            }
        }

        /// <summary>
        /// 移除所有事件
        /// </summary>
        public void RemoveAllListener()
        {
            foreach (var listeners in EventDict.Values)
            {
                listeners.Clear();
            }
            EventDict.Clear();
        }

        public void Notify(string eventName, T args, float delayTime = 0)
        {
            List<Action<T>> listeners = null;
            if (EventDict.TryGetValue(eventName, out listeners))
            {
                if (delayTime == 0)
                {
                    for (int i = 0; i < listeners.Count; i++)
                    {
                        Action<T> listener = listeners[i];
                        try { listener?.Invoke(args); }
                        catch (Exception e) { Logger.Error(e); }
                    }
                }
                else
                {
                    Delay oDelayEvent = new Delay(eventName, args, delayTime, Notify);
                    oDelayEvent.Notify();
                }
            }
        }

        protected class Delay : IDisposable
        {
            public IDriverSer Driver
            {
                get
                {
                    if (_Driver == null)
                        _Driver = IoC.Resolve<IDriverSer>();
                    return _Driver;
                }
            }
            private IDriverSer _Driver;

            private float UseTime { get; set; }
            private float DelayTime { get; set; }
            private string CallEvent { get; set; }
            private T EventArgs { get; set; }
            public Action<string, T, float> OnNotify { get; set; }

            public Delay(string callEvent, T eventArgs,
                float delayTime, Action<string, T, float> onNotify)
            {
                CallEvent = callEvent;
                EventArgs = eventArgs;
                DelayTime = delayTime;
                OnNotify = onNotify;
                UseTime = 0;
            }

            public void Notify()
            {
                Driver.Subscribe(Update);
            }

            public void Update(float deltaTime)
            {
                UseTime = Clamp(UseTime + deltaTime, 0, DelayTime);
                if (UseTime == DelayTime)
                {
                    try { OnNotify?.Invoke(CallEvent, EventArgs, 0); }
                    catch (Exception e) { IoC.Resolve<ILoggerSer>().Error(e); }
                    Driver.Unsubscribe(Update); Dispose();
                }
            }

            private float Clamp(float value, float min, float max)
            {
                if (value < min) return min;
                if (value > max) return max;
                return value;
            }

            private bool IsDispose = false;
            public void Dispose()
            {
                if (!IsDispose)
                {
                    IsDispose = true;
                    GC.SuppressFinalize(this);
                }
            }
        }
    }
}

