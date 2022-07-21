using SMCore.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Objects
{
    public interface IObjectEvent : IObject
    {
        void AddListener(Action<IEventArgs> listener);
        void RemoveListener(Action<IEventArgs> listener);
        void RemoveAllListener();
        void Notify(IEventArgs args, float delayTime = 0);
        string NotifyAllName { get; }
        /// <summary>全盘通知</summary>
        void OverallNotify();
        void AddOverallListener(Action<IEventArgs> listener);

        void RemoveOverallListener(Action<IEventArgs> listener);

        void RemoveAllOverallListener();
    }
}
