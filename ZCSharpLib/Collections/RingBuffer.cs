using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Collections
{
    public class RingBuffer<T>
    {
        // 环形Buffer
        protected int _rwLength;
        protected int _bufferSize;
        protected int _readPosition = -1;
        protected int _writePosition = -1;
        protected T[] _buffer;
        public int rwLenght => _rwLength;
        public int BufferSize => _bufferSize;
        public int ReadPosition => _readPosition;
        public int WritePosition => _writePosition;
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _bufferSize)
                    return default(T);
                return _buffer[index];
            }
        }

        public RingBuffer(int bufferSize)
        {
            _bufferSize = bufferSize;
            _buffer = new T[_bufferSize];
        }

        public void Enqueue(T shift)
        {
            if (_rwLength < BufferSize)
            {
                // 写在赋值前面保证写入索引可以获取到当前赋值的值
                _rwLength += 1; _writePosition += 1;
                if (_writePosition >= BufferSize) _writePosition = 0;

                _buffer[_writePosition] = shift;
            }
            else throw new ArgumentOutOfRangeException($"环形Buffer越界, 请清理一部分数据! _rwLenght={_rwLength}");
        }

        public T Dequeue()
        {
            if (_rwLength > 0)
            {
                // 写在取值前面保证读取索引可以获取到当前读取的值
                _rwLength -= 1; _readPosition += 1;
                if (_readPosition >= BufferSize) _readPosition = 0;

                return _buffer[_readPosition];
            }
            else throw new ArgumentOutOfRangeException($"环形Buffer为空, 请存取数据后在读! _rwLenght={_rwLength}");
        }
    }
}
