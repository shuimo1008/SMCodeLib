﻿using System;
using System.Collections.Generic;
using ZCSharpLib.Utils;
using ZCSharpLib.Times;

namespace ZCSharpLib.Events
{
    public class Event
    {
        protected Dictionary<string, List<Action<IEventArgs>>> EventDict { get; set; }

        public Event()
        {
            EventDict = new Dictionary<string, List<Action<IEventArgs>>>();
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听者</param>
        public void AddListener(string eventName, Action<IEventArgs> listener)
        {
            //App.Logger.Info("参数1：" + eventName);
            if (listener == null) return;
            List<Action<IEventArgs>> listeners = null;
            if (!EventDict.TryGetValue(eventName, out listeners))
            {
                listeners = new List<Action<IEventArgs>>();
                EventDict.Add(eventName, listeners);
            }
            if (!listeners.Contains(listener)) listeners.Add(listener);
        }

        /// <summary>
        /// 移除指定的事件
        /// </summary>
        /// <param name="eventName">指定的事件名</param>
        /// <param name="listener">指定的事件</param>
        public void RemoveListener(string eventName, Action<IEventArgs> listener)
        {
            if (listener == null) return;
            List<Action<IEventArgs>> listeners = null;
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
            List<Action<IEventArgs>> listeners = null;
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

        public void Notify(string eventName, IEventArgs args, float delayTime = 0)
        {
            List<Action<IEventArgs>> listeners = null;
            if (EventDict.TryGetValue(eventName, out listeners))
            {
                if (delayTime == 0)
                {
                    for (int i = 0; i < listeners.Count; i++)
                    {
                        Action<IEventArgs> listener = listeners[i];
                        try { listener(args); }
                        catch (Exception e) { App.Error(e); }
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
            private float UseTime { get; set; }
            private float DelayTime { get; set; }
            private string CallEvent { get; set; }
            private IEventArgs EventArgs { get; set; }
            public Action<string, IEventArgs, float> OnNotify { get; set; }

            public Delay(string callEvent, IEventArgs eventArgs,
                float delayTime, Action<string, IEventArgs, float> onNotify)
            {
                CallEvent = callEvent;
                EventArgs = eventArgs;
                DelayTime = delayTime;
                OnNotify = onNotify;
                UseTime = 0;
            }

            public void Notify()
            {
                App.SubscribeUpdate(Update);
            }

            public void Update(float deltaTime)
            {
                UseTime = MathUtils.Clamp(UseTime + deltaTime, 0, DelayTime);
                if (UseTime == DelayTime)
                {
                    try
                    {
                        OnNotify?.Invoke(CallEvent, EventArgs, 0);
                    }
                    catch (Exception e)
                    {
                        App.Error(e);
                    }
                    App.UnsubscribeUpdate(Update);
                    Dispose();
                }
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

