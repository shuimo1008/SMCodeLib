using System;
using System.Collections.Generic;

namespace ZCSharpLib.Cores
{
    public class ObjectList<T> where T : class
    {
        protected List<T> m_list;

        public T this[int index]
        {
            get
            {
                return m_list[index];
            }
            set
            {
                m_list[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return m_list.Count;
            }
        }

        public ObjectList()
        {
            m_list = new List<T>();
        }

        public virtual void Add(T item)
        {
            lock (m_list)
            {
                m_list.Add(item);
            }
        }

        public virtual void Remove(T item)
        {
            lock (m_list)
            {
                m_list.Remove(item);
            }
        }

        public virtual void Enqueue(T item)
        {
            Add(item);
        }

        public virtual T Dequeue()
        {
            lock (m_list)
            {
                if (m_list.Count > 0)
                {
                    T t = m_list[0];
                    m_list.RemoveAt(0);
                    return t;
                }
                else
                {
                    throw new System.ArgumentNullException("列表为空!");
                }
            }
        }

        public virtual T Peek()
        {
            lock (m_list)
            {
                if (m_list.Count > 0)
                {
                    return m_list[0];
                }
                else
                {
                    throw new System.ArgumentNullException("列表为空!");
                }
            }
        }

        public virtual T Find(Predicate<T> match)
        {
            lock (m_list)
            {
                return m_list.Find(match);
            }
        }

        public virtual bool Contains(T item)
        {
            lock (m_list)
            {
                return m_list.Contains(item);
            }
        }

        public virtual void Clear()
        {
            lock (m_list)
            {
                m_list.Clear();
            }
        }

        public void CopyList(ref T[] array)
        {
            lock (m_list)
            {
                array = new T[m_list.Count];
                m_list.CopyTo(array);
            }
        }
    }
}
