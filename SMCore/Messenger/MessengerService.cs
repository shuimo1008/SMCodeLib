using SMCore.Logger;
using SMCore.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Messenger
{
    public class MessengerService : ObjectBase, IMessengerService
    {
        public List<Action<IMessenger>> Subscribers
        {
            get
            {
                if (_Subscribers == null)
                    _Subscribers = new List<Action<IMessenger>>();
                return _Subscribers;
            }
        }
        private List<Action<IMessenger>> _Subscribers;

        public void Publish(IMessenger messenger)
        {
            foreach (var subscriber in Subscribers)
            {
                try
                {
                    subscriber?.Invoke(messenger);
                }
                catch (Exception e) { IoC.Resolve<ILoggerService>().Error(e); }
            }
        }

        public void Subscribe(Action<IMessenger> subscriber)
        {
            if (!Subscribers.Contains(subscriber))
                Subscribers.Add(subscriber);
        }

        public void Unsubscribe(Action<IMessenger> subscriber)
        {
            if (Subscribers.Contains(subscriber))
                Subscribers.Remove(subscriber);
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            Subscribers.Clear();
        }
    }
}
