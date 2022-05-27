using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Events
{
    public interface IEventDispatcher
    {
        void AddListener(string eventType, Action handler);
        void AddListener<T>(string eventType, Action<T> handler);
        void AddListener<T, U>(string eventType, Action<T, U> handler);
        void AddListener<T, U, V>(string eventType, Action<T, U, V> handler);
        void RemoveListener(string eventType, Action handler);
        void RemoveListener<T>(string eventType, Action<T> handler);
        void RemoveListener<T, U>(string eventType, Action<T, U> handler);
        void RemoveListener<T, U, V>(string eventType, Action<T, U, V> handler);
        void Dispatch(string eventType);
        void Dispatch<T>(string eventType, T t);
        void Dispatch<T, U>(string eventType, T t, U u);
        void Dispatch<T, U, V>(string eventType, T t, U u, V v);
    }
}
