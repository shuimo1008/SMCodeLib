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
        private Stack<T> mPool;

        public ObjectPool(int capacity)
        {
            mPool = new Stack<T>(capacity);
        }

        public void Push(T item)
        {
            if (item == null)
            {
                throw new ArgumentException(string.Format( "Items added to a {0} cannot be null", typeof(T).Name));
            }
            lock (mPool)
            {
                mPool.Push(item);
            }
        }

        public T Pop()
        {
            lock (mPool)
            {
                T t = mPool.Pop();
                return t;
            }
        }

        public int Count
        {
            get { return mPool.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var item in mPool)
            {
                yield return item;
            }
        }
    }
}
