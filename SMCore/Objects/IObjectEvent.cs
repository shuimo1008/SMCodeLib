using SMCore.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Objects
{
    public interface IObjectEvent<T> : IObject where T : IEventArgs
    {
        void AddListener(Action<T> listener);
        void RemoveListener(Action<T> listener);
        void RemoveAllListener();
        void Notify(T args, float delayTime = 0);
        string NotifyAllName { get; }
        /// <summary>全盘通知</summary>
        void OverallNotify();
        void AddOverallListener(Action<T> listener);

        void RemoveOverallListener(Action<T> listener);

        void RemoveAllOverallListener();
    }
}
