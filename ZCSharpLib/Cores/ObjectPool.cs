using System;
using System.Collections;
using System.Collections.Generic;

namespace ZCSharpLib.Cores
{
    public interface IPool
    {
        void Reset();
    }

    public class ObjectPool<T> : IEnumerable where T : class
    {
        private Stack<T> objectPool;

        private object sync = new object();

        public ObjectPool(int capacity)
        {
            objectPool = new Stack<T>(capacity);
        }

        public void Push(T item)
        {
            if (item == null)
                throw new ArgumentException(string.Format( "Items added to a {0} cannot be null", typeof(T).Name));
            lock (sync) { objectPool.Push(item); }
        }

        public T Pop()
        {
            lock (sync) { return objectPool.Pop(); }
        }

        public int Count => objectPool.Count;
        public IEnumerator GetEnumerator()
        {
            foreach (var item in objectPool)
                yield return item;
        }
    }
}
