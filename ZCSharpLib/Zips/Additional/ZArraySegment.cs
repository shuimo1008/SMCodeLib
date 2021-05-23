using System;
using System.Collections;
using System.Collections.Generic;

namespace ZCSharpLib.Zips.Additional
{
    /// <summary>Delimits a section of a one-dimensional array.</summary>
    /// <typeparam name="T">The type of the elements in the array segment.</typeparam>
    [Serializable]
    public struct ZArraySegment<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private T[] _array;
        private int _offset;
        private int _count;

        /// <summary>Initializes a new instance of the <see cref="T:System.ArraySegment`1" /> structure that delimits all the elements in the specified array.</summary>
        /// <param name="array">The array to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is <see langword="null" />.</exception>
        public ZArraySegment(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            this._array = array;
            this._offset = 0;
            this._count = array.Length;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ArraySegment`1" /> structure that delimits the specified range of the elements in the specified array.</summary>
        /// <param name="array">The array containing the range of elements to delimit.</param>
        /// <param name="offset">The zero-based index of the first element in the range.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="offset" /> and <paramref name="count" /> do not specify a valid range in <paramref name="array" />.</exception>
        public ZArraySegment(T[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "ArgumentOutOfRange_NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "ArgumentOutOfRange_NeedNonNegNum");
            if (array.Length - offset < count)
                throw new ArgumentException("Argument_InvalidOffLen");
            this._array = array;
            this._offset = offset;
            this._count = count;
        }

        /// <summary>Gets the original array containing the range of elements that the array segment delimits.</summary>
        /// <returns>The original array that was passed to the constructor, and that contains the range delimited by the <see cref="T:System.ArraySegment`1" />.</returns>
        public T[] Array
        {
            get
            {
                return this._array;
            }
        }

        /// <summary>Gets the position of the first element in the range delimited by the array segment, relative to the start of the original array.</summary>
        /// <returns>The position of the first element in the range delimited by the <see cref="T:System.ArraySegment`1" />, relative to the start of the original array.</returns>
        public int Offset
        {
            get
            {
                return this._offset;
            }
        }

        /// <summary>Gets the number of elements in the range delimited by the array segment.</summary>
        /// <returns>The number of elements in the range delimited by the <see cref="T:System.ArraySegment`1" />.</returns>
        public int Count
        {
            get
            {
                return this._count;
            }
        }

        /// <summary>Returns the hash code for the current instance.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            if (this._array != null)
                return this._array.GetHashCode() ^ this._offset ^ this._count;
            return 0;
        }

        /// <summary>Determines whether the specified object is equal to the current instance.</summary>
        /// <param name="obj">The object to be compared with the current instance.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object is a <see cref="T:System.ArraySegment`1" /> structure and is equal to the current instance; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ZArraySegment<T>)
                return this.Equals((ZArraySegment<T>)obj);
            return false;
        }

        /// <summary>Determines whether the specified <see cref="T:System.ArraySegment`1" /> structure is equal to the current instance.</summary>
        /// <param name="obj">The structure to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true" /> if the specified <see cref="T:System.ArraySegment`1" /> structure is equal to the current instance; otherwise, <see langword="false" />.</returns>
        public bool Equals(ZArraySegment<T> obj)
        {
            if (obj._array == this._array && obj._offset == this._offset)
                return obj._count == this._count;
            return false;
        }

        /// <summary>Indicates whether two <see cref="T:System.ArraySegment`1" /> structures are equal.</summary>
        /// <param name="a">The  structure on the left side of the equality operator.</param>
        /// <param name="b">The structure on the right side of the equality operator.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="a" /> is equal to <paramref name="b" />; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(ZArraySegment<T> a, ZArraySegment<T> b)
        {
            return a.Equals(b);
        }

        /// <summary>Indicates whether two <see cref="T:System.ArraySegment`1" /> structures are unequal.</summary>
        /// <param name="a">The structure on the left side of the inequality operator.</param>
        /// <param name="b">The structure on the right side of the inequality operator.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="a" /> is not equal to <paramref name="b" />; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(ZArraySegment<T> a, ZArraySegment<T> b)
        {
            return !(a == b);
        }

        T IList<T>.this[int index]
        {
            get
            {
                if (this._array == null)
                    throw new InvalidOperationException("InvalidOperation_NullArray");
                if (index < 0 || index >= this._count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this._array[this._offset + index];
            }
            set
            {
                if (this._array == null)
                    throw new InvalidOperationException("InvalidOperation_NullArray");
                if (index < 0 || index >= this._count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                this._array[this._offset + index] = value;
            }
        }
        
        int IList<T>.IndexOf(T item)
        {
            if (this._array == null)
                throw new InvalidOperationException("InvalidOperation_NullArray");
            int num = System.Array.IndexOf<T>(this._array, item, this._offset, this._count);
            if (num < 0)
                return -1;
            return num - this._offset;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        T this[int index]
        {
            get
            {
                if (this._array == null)
                    throw new InvalidOperationException("InvalidOperation_NullArray");
                if (index < 0 || index >= this._count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this._array[this._offset + index];
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            if (this._array == null)
                throw new InvalidOperationException("InvalidOperation_NullArray");
            return System.Array.IndexOf<T>(this._array, item, this._offset, this._count) >= 0;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (this._array == null)
                throw new InvalidOperationException("InvalidOperation_NullArray");
            System.Array.Copy((System.Array)this._array, this._offset, (System.Array)array, arrayIndex, this._count);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (this._array == null)
                throw new InvalidOperationException("InvalidOperation_NullArray");
            return (IEnumerator<T>)new ZArraySegment<T>.ArraySegmentEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this._array == null)
                throw new InvalidOperationException("InvalidOperation_NullArray");
            return (IEnumerator)new ZArraySegment<T>.ArraySegmentEnumerator(this);
        }

        [Serializable]
        private sealed class ArraySegmentEnumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private T[] _array;
            private int _start;
            private int _end;
            private int _current;

            internal ArraySegmentEnumerator(ZArraySegment<T> arraySegment)
            {
                this._array = arraySegment._array;
                this._start = arraySegment._offset;
                this._end = this._start + arraySegment._count;
                this._current = this._start - 1;
            }

            public bool MoveNext()
            {
                if (this._current >= this._end)
                    return false;
                ++this._current;
                return this._current < this._end;
            }

            public T Current
            {
                get
                {
                    if (this._current < this._start)
                        throw new InvalidOperationException("InvalidOperation_EnumNotStarted");
                    if (this._current >= this._end)
                        throw new InvalidOperationException("InvalidOperation_EnumEnded");
                    return this._array[this._current];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (object)this.Current;
                }
            }

            void IEnumerator.Reset()
            {
                this._current = this._start - 1;
            }

            public void Dispose()
            {
            }
        }
    }
}
