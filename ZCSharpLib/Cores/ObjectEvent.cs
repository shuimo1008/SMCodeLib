using System;
using ZCSharpLib.Events;
using ZCSharpLib.Objects;

namespace ZCSharpLib.Cores
{
    public abstract class ObjectEvent : ObjectBase
    {
        protected Event Event { get; private set; }

        protected string EventCall
        {
            get
            {
                return GetHashCode().ToString();
            }
        }

        public ObjectEvent()
        {
            Event = new Event();
        }

        #region 事件
        public virtual void AddListener(Action<IEventArgs> listener)
        {
            Event.AddListener(EventCall, listener);
        }

        public virtual void RemoveListener(Action<IEventArgs> listener)
        {
            Event.RemoveListener(EventCall, listener);
        }

        public virtual void RemoveAllListener()
        {
            Event.RemoveAllListener(EventCall);
        }

        public virtual void Notify(IEventArgs args, float delayTime = 0)
        {
            Event.Notify(EventCall, args, delayTime);
        }
        #endregion

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            Event.RemoveAllListener();
        }
    }
}
