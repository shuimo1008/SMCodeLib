using System;
using System.Collections.Generic;

namespace ZCSharpLib.Cores
{
    public class ObjectList<T> where T : class
    {
        protected object sync = new object();
        protected List<T> list;

        public T this[int index]
        {
            get
            {
                lock (sync) { return list[index]; }
            }
            set
            {
                lock (sync) { list[index] = value; }
            }
        }

        public int Count
        {
            get
            {
                lock (sync) { return list.Count; }
            }
        }

        public ObjectList()
        {
            list = new List<T>();
        }

        public virtual void Add(T item)
        {
            lock (sync) { list.Add(item); }
        }

        public virtual void Remove(T item)
        {
            lock (sync) { list.Remove(item); }
        }

        public virtual void Enqueue(T item) => Add(item);

        public virtual T Dequeue()
        {
            lock (sync)
            {
                if (list.Count > 0)
                {
                    T t = list[0];
                    list.RemoveAt(0);
                    return t;
                }
                else throw new ArgumentNullException("列表为空!");
            }
        }

        public virtual T Peek()
        {
            lock (sync)
            {
                if (list.Count > 0)
                    return list[0];
                else throw new ArgumentNullException("列表为空!");
            }
        }

        public virtual T Find(Predicate<T> match)
        {
            lock (sync) { return list.Find(match); }
        }

        public virtual bool Contains(T item)
        {
            lock (sync) { return list.Contains(item); }
        }

        public virtual void Clear()
        {
            lock (sync) { list.Clear(); }
        }

        public void CopyList(ref T[] array)
        {
            lock (sync)
            {
                array = new T[list.Count];
                list.CopyTo(array);
            }
        }
    }
}
