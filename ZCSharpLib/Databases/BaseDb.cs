using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZCSharpLib.Databases
{
    public enum ConnectionType
    {
        NONE,
        OPEN,
        CLOSE,
    }

    public abstract class BaseDb
    {
        /// <summary>
        /// 数据库连接状态
        /// </summary>
        public abstract ConnectionType ConnectionType { get; }

        /// <summary>
        /// 连接到数据库
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>是否连接成功</returns>
        public abstract bool Connect();

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns>是否关闭成功</returns>
        public abstract bool Disconnect();

        /// <summary>
        /// 数据库查询
        /// </summary>
        public abstract DatabaseReader ExecuteQuery(string query);
    }


    /// <summary>
    /// 数据读取者
    /// </summary>
    public class DatabaseReader : IEnumerable
    {
        public class DatabaseElement
        {
            private object[] m_Items;

            public DatabaseElement(int count)
            {
                m_Items = new object[count];
            }

            public void SetValue(int index, object obj)
            {
                if (index < m_Items.Length)
                {
                    m_Items[index] = obj;
                }
            }

            public object GetValue(int index)
            {
                object obj = null;
                if (index < m_Items.Length)
                {
                    obj = m_Items[index];
                }
                return obj;
            }
        }

        /// <summary>
        /// 字段个数
        /// </summary>
        public int FieldCount { get; private set; }

        /// <summary>
        /// 元素个数
        /// </summary>
        public int ElementCount
        {
            get
            {
                return m_Items.Count;
            }
        }

        /// <summary>
        /// 字段内容
        /// </summary>
        private List<DatabaseElement> m_Items;

        public DatabaseReader(int fieldCount)
        {
            FieldCount = fieldCount;
            m_Items = new List<DatabaseElement>();
        }

        /// <summary>
        /// 创建一个新的数据集
        /// </summary>
        public DatabaseElement CreateNew()
        {
            DatabaseElement oItem = new DatabaseElement(FieldCount);
            return oItem;
        }

        /// <summary>
        /// 设置元素集合
        /// </summary>
        /// <param name="oItem"></param>
        public void SetElement(DatabaseElement oItem)
        {
            m_Items.Add(oItem);
        }

        public IEnumerator GetEnumerator()
        {
            return new DatabaseReaderEnumerator(this);
        }

        private class DatabaseReaderEnumerator : IEnumerator
        {
            private DatabaseReader databaseReader;

            private int Position = -1;

            public DatabaseReaderEnumerator(DatabaseReader databaseReader)
            {
                this.databaseReader = databaseReader;
            }

            public object Current
            {
                get
                {
                    return databaseReader.m_Items[Position];
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                Position = Position + 1;
                return Position < databaseReader.m_Items.Count;
            }

            public void Reset()
            {
                Position = -1;
            }
        }
    }

}
