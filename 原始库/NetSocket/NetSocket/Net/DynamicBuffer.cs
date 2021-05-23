using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetSocket
{
    public class DynamicBuffer
    {
        public byte[] Buffer { get; private set; }

        public int DataCount { get; private set; }


        public DynamicBuffer() : this(0)
        {
        }

        public DynamicBuffer(int size)
        {
            Buffer = new byte[size];
        }

        public DynamicBuffer(byte[] bytes) : this(bytes, 0, bytes.Length) { }

        public DynamicBuffer(byte[] bytes, int offset, int count)
        {
            Buffer = new byte[count];
            Array.Copy(bytes, offset, Buffer, 0, count);
        }

        public int GetIdleCount()
        {
            return Buffer.Length - DataCount;
        }

        public void Clear()
        {
            DataCount = 0;
        }

        public void Clear(int count)
        {
            if (count >= DataCount)
            {
                DataCount = 0;
            }
            else
            {
                for (int i = 0; i < DataCount - count; i++)
                {
                    Buffer[i] = Buffer[count + i];
                }
                DataCount = DataCount - count;
            }
        }

        public void ModifySize(int size)
        {
            if (Buffer.Length < size)
            {
                byte[] tmpBuffer = new byte[size];
                Array.Copy(Buffer, 0, tmpBuffer, 0, DataCount);
                Buffer = tmpBuffer;
            }
        }
        #region 写入

        public void WriteByte(byte value)
        {
            byte[] bytes = new byte[1] { value };
            WriteBytes(bytes);
        }

        public void WriteBool(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteInt16(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteUInt16(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteInt32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteUInt32(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteInt64(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteUInt64(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteDouble(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteUTF8(string value)
        {
            value = value == null ? string.Empty : value;
            byte[] contentBytes = Encoding.UTF8.GetBytes(value);
            byte[] headBytes = BitConverter.GetBytes(contentBytes.Length);
            WriteBytes(headBytes);
            WriteBytes(contentBytes);
        }

        public void WriteBytes(byte[] oBytes)
        {
            WriteBytes(oBytes, 0, oBytes.Length);
        }

        public void WriteBytes(byte[] oBytes, int offset, int count)
        {
            if (count > GetIdleCount())
            {
                int totalSize = Buffer.Length - GetIdleCount() + count;
                byte[] tmpBytes = new byte[totalSize];
                Array.Copy(Buffer, 0, tmpBytes, 0, DataCount);
                Buffer = tmpBytes;
            }
            //if (BitConverter.IsLittleEndian) Array.Reverse(oBytes);
            Array.Copy(oBytes, offset, Buffer, DataCount, count);
            DataCount = DataCount + count;
        }

        #endregion

        #region 读取

        public byte ReadByte()
        {
            byte obyte = ReadBytes(1)[0];
            return obyte;
        }

        public bool ReadBool()
        {
            byte[] bytes = ReadBytes(1);
            bool oBool = BitConverter.ToBoolean(bytes, 0);
            return oBool;
        }

        public short ReadInt16()
        {
            byte[] bytes = ReadBytes(2);
            short oShort = BitConverter.ToInt16(bytes, 0);
            return oShort;
        }

        public ushort ReadUInt16()
        {
            byte[] bytes = ReadBytes(2);
            ushort oShort = BitConverter.ToUInt16(bytes, 0);
            return oShort;
        }

        public int ReadInt32()
        {
            byte[] bytes = ReadBytes(4);
            int oInt = BitConverter.ToInt32(bytes, 0);
            return oInt;
        }

        public uint ReadUInt32()
        {
            byte[] bytes = ReadBytes(4);
            uint oUInt = BitConverter.ToUInt32(bytes, 0);
            return oUInt;
        }

        public long ReadInt64()
        {
            byte[] bytes = ReadBytes(8);
            long oLong = BitConverter.ToInt64(bytes, 0);
            return oLong;
        }

        public ulong ReadUInt64()
        {
            byte[] bytes = ReadBytes(8);
            ulong oULong = BitConverter.ToUInt64(bytes, 0);
            return oULong;
        }

        public float ReadFloat()
        {
            byte[] bytes = ReadBytes(4);
            float oSingle = BitConverter.ToSingle(bytes, 0);
            return oSingle;
        }

        public double ReadDouble()
        {
            byte[] bytes = ReadBytes(8);
            double oDouble = BitConverter.ToDouble(bytes, 0);
            return oDouble;
        }

        public string ReadUTF8()
        {
            int lenght = ReadInt32();
            byte[] bytes = ReadBytes(lenght);
            string content = Encoding.UTF8.GetString(bytes, 0, lenght);
            return content;
        }

        public byte[] ReadBytes(int count)
        {
            return ReadBytes(Buffer, DataCount, count);
        }

        public byte[] ReadBytes(byte[] oBytes, int offset, int count)
        {
            if (GetIdleCount() < count)
            {
                throw new Exception(string.Format("DynamicBuffer 数据越界，原始大小{0} 剩余大小{1} 截取大小{2}", Buffer.Length, GetIdleCount(), count));
            }
            byte[] bytes = new byte[count];
            Array.Copy(oBytes, offset, bytes, 0, count);
            //if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            DataCount = DataCount + count;
            return bytes;
        }
        #endregion
    }
}
