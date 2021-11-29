using System;
using ZCSharpLib.Events;
using ZCSharpLib.Objects;

namespace ZCSharpLib.Cores
{
    public abstract class ObjectEvent : ObjectBase
    {
        protected Event Event { get; private set; }

        protected int HashID
        {
            get
            {
                if (_hashID == 0)
                    _hashID = GetHashCode();
                return _hashID;
            }
        }
        protected int _hashID;

        public ObjectEvent()
        {
            Event = new Event();
        }

        #region 事件
        public virtual void AddListener(Action<IEventArgs> listener)
        {
            Event.AddListener(HashID.ToString(), listener);
        }

        public virtual void RemoveListener(Action<IEventArgs> listener)
        {
            Event.RemoveListener(HashID.ToString(), listener);
        }

        public virtual void RemoveAllListener()
        {
            Event.RemoveAllListener(HashID.ToString());
        }

        public virtual void Notify(IEventArgs args, float delayTime = 0)
        {
            Event.Notify(HashID.ToString(), args, delayTime);
        }

        public string NotifyAllName
        {
            get
            {
                if (string.IsNullOrEmpty(_notfiyAllName))
                    _notfiyAllName = $"{HashID}_All";
                return _notfiyAllName;
            }
        }
        protected string _notfiyAllName;

        /// <summary>全盘通知</summary>
        public virtual void OverallNotify()
        {
            Event.Notify(NotifyAllName, null);
        }

        public virtual void AddOverallListener(Action<IEventArgs> listener)
        {
            Event.AddListener(NotifyAllName, listener);
        }

        public virtual void RemoveOverallListener(Action<IEventArgs> listener)
        {
            Event.RemoveListener(NotifyAllName, listener);
        }

        public virtual void RemoveAllOverallListener()
        {
            Event.RemoveAllListener(NotifyAllName);
        }
        #endregion

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            Event.RemoveAllListener();
        }
    }
}
