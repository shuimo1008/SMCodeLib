using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSocket.Core
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

    public class ObjectSingleList<T> : ObjectList<T> where T : class
    {
        public override void Add(T item)
        {
            lock (m_list)
            {
                if(!m_list.Contains(item)) m_list.Add(item);
            }
        }

        public override void Remove(T item)
        {
            lock (m_list)
            {
                if(m_list.Contains(item)) m_list.Remove(item);
            }
        }
    }
}
