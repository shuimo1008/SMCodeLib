using System;
using System.Collections.Generic;
using SMCore.Objects;

namespace SMCore.Events
{
    public class EventDispatcher : IEventDispatcher
    {
        private Dictionary<string, Delegate> Events { get; set; }

        public EventDispatcher()
        {
            Events = new Dictionary<string, Delegate>();
        }

        private Delegate OnAddListenerAdding(string eventType, Delegate linstener)
        {
            Delegate @delegate = null;
            if (!Events.TryGetValue(eventType, out @delegate))
            {
                Events.Add(eventType, @delegate);
            }
            if (@delegate != null && linstener.GetType() != @delegate.GetType())
            {
                throw new Exception(string.Format("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", eventType, @delegate.GetType().Name, linstener.GetType().Name));
            }
            return @delegate;
        }

        public void AddListener(string eventType, Action handler)
        {
            Action a = (Action)OnAddListenerAdding(eventType, handler);
            Events[eventType] = (Action)Delegate.Combine(a, handler);
        }

        public void AddListener<T>(string eventType, Action<T> handler)
        {
            Action<T> a = (Action<T>)OnAddListenerAdding(eventType, handler);
            Events[eventType] = (Action<T>)Delegate.Combine(a, handler);
        }

        public void AddListener<T, U>(string eventType, Action<T, U> handler)
        {
            Action<T, U> a = (Action<T, U>)OnAddListenerAdding(eventType, handler);
            Events[eventType] = (Action<T, U>)Delegate.Combine(a, handler);
        }

        public void AddListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            Action<T, U, V> a = (Action<T, U, V>)OnAddListenerAdding(eventType, handler);
            Events[eventType] = (Action<T, U, V>)Delegate.Combine(a, handler);
        }

        private void OnListenerRemoving(string eventType, Delegate linstener)
        {
            Delegate @delegate = null;
            if (!Events.TryGetValue(eventType, out @delegate))
            {
                throw new Exception(string.Format("Attempting to remove listener for type \"{0}\" but Messenger doesn't know about this event type.", eventType));
            }
            if (@delegate == null)
            {
                throw new Exception(string.Format("Attempting to remove listener with for event type \"{0}\" but current listener is null.", eventType));
            }
            if (@delegate != null && @delegate.GetType() != linstener.GetType())
            {
                throw new Exception(string.Format("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", eventType, @delegate.GetType().Name, linstener.GetType().Name));
            }
        }

        public void OnListenerRemoved(string eventType)
        {
            Delegate @delegate = null;
            if (Events.TryGetValue(eventType, out @delegate) && @delegate == null)
            {
                Events.Remove(eventType);
            }
        }

        public void RemoveListener(string eventType, Action handler)
        {
            OnListenerRemoving(eventType, handler);
            Events[eventType] = (Action)Delegate.Remove((Action)Events[eventType], handler);
            OnListenerRemoved(eventType);
        }

        public void RemoveListener<T>(string eventType, Action<T> handler)
        {
            OnListenerRemoving(eventType, handler);
            Events[eventType] = (Action<T>)Delegate.Remove((Action<T>)Events[eventType], handler);
            OnListenerRemoved(eventType);
        }

        public void RemoveListener<T, U>(string eventType, Action<T, U> handler)
        {
            OnListenerRemoving(eventType, handler);
            Events[eventType] = (Action<T, U>)Delegate.Remove((Action<T, U>)Events[eventType], handler);
            OnListenerRemoved(eventType);
        }

        public void RemoveListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            OnListenerRemoving(eventType, handler);
            Events[eventType] = (Action<T, U, V>)Delegate.Remove((Action<T, U, V>)Events[eventType], handler);
            OnListenerRemoved(eventType);
        }

        public void Notify(string eventType)
        {
            Delegate @delegate = null;
            if (Events.TryGetValue(eventType, out @delegate))
            {
                Action callback = @delegate as Action;
                callback();
            }
        }

        public void Notify<T>(string eventType, T t)
        {
            Delegate @delegate = null;
            if (Events.TryGetValue(eventType, out @delegate))
            {
                Action<T> callback = @delegate as Action<T>;
                callback(t);
            }
        }

        public void Notify<T, U>(string eventType, T t, U u)
        {
            Delegate @delegate = null;
            if (Events.TryGetValue(eventType, out @delegate))
            {
                Action<T, U> callback = @delegate as Action<T, U>;
                callback(t, u);
            }
        }

        public void Notify<T, U, V>(string eventType, T t, U u, V v)
        {
            Delegate @delegate = null;
            if (Events.TryGetValue(eventType, out @delegate))
            {
                Action<T, U, V> callback = @delegate as Action<T, U, V>;
                callback(t, u, v);
            }
        }

        public void Dispose()
        {
            Events.Clear();
        }
    }
}
