using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public class NewsSer : INewsSer
    {
        public Dictionary<Type, object> Subscribers
        {
            get
            {
                if (_Subscribers == null)
                    _Subscribers = new Dictionary<Type, object>();
                return _Subscribers;
            }
        }
        private Dictionary<Type, object> _Subscribers;

        public T Publish<T>(T t) where T : INews
        {
            if (Subscribers.TryGetValue(typeof(T), out var v))
                (v as Action<T>)?.Invoke(t);
            return t;
        }

        public void Subscribe<T>(Action<T> subscriber) where T : INews
        {
            if (!Subscribers.TryGetValue(typeof(T), out var v))
                Subscribers.Add(typeof(T), v = subscriber);
        }

        public void Unsubscribe<T>(Action<T> subscriber) where T : INews
        {
            if(Subscribers.TryGetValue(typeof(T), out var v))
                Subscribers.Remove(typeof(T));
        }
    }
}
