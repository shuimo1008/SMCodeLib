using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ZCSharpLib.Nets
{
    public class ByteBuffer
    {
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// 数组有效数据位置
        /// </summary>
        public int Position { get; private set; }


        public ByteBuffer() : this(0)
        {
        }

        public ByteBuffer(int size)
        {
            Bytes = new byte[size];
        }

        public ByteBuffer(byte[] bytes) : this(bytes, 0, bytes.Length) { }

        public ByteBuffer(byte[] bytes, int offset, int count)
        {
            Bytes = new byte[count];
            Array.Copy(bytes, offset, Bytes, 0, count);
        }

        /// <summary>
        /// 获得空余的字节数
        /// </summary>
        public int Remain
        {
            get
            {
                return Bytes.Length - Position;
            }
        }

        /// <summary>
        /// 清理指定大小的数据
        /// </summary>
        /// <param name="count"></param>
        public void Clear(int count = -1)
        {
            //如果需要清理的数据大于现有数据大小，则全部清理
            if (count >= Position || count == -1)
            {
                Position = 0;
            }
            else
            {
                //否则后面的数据往前移
                for (int i = 0; i < Position - count; i++)
                    Bytes[i] = Bytes[count + i];
                Position = Position - count;
            }
        }

        /// <summary>
        /// 修改缓存大小
        /// </summary>
        /// <param name="size"></param>
        public void ModifySize(int size) 
        {
            if (Bytes.Length < size)
            {
                byte[] tmpBuffer = new byte[size];
                Array.Copy(Bytes, 0, tmpBuffer, 0, Position); //复制以前的数据
                Bytes = tmpBuffer; //替换
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

        public void WriteBytes(byte[] oBytes, int offset, int length)
        {
            // 缓存不够需要重新申请
            if (length > Remain) // 写入的字节数大于剩余的字节数
            {
                int totalSize = Bytes.Length - Remain + length; // 现在需要的总字节数
                byte[] tmpBytes = new byte[totalSize];
                Array.Copy(Bytes, 0, tmpBytes, 0, Position); //复制以前的数据
                Bytes = tmpBytes;
            }
            // 如果是大端模式则转换为小端模式进行写入
            if (!BitConverter.IsLittleEndian) Array.Reverse(oBytes);
            Array.Copy(oBytes, offset, Bytes, Position, length);  
            Position = Position + length;
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
            return ReadBytes(Bytes, Position, count);
        }

        public byte[] ReadBytes(byte[] oBytes, int offset, int length)
        {
            if (Remain < length)
            {
                throw new Exception(string.Format("ByteArray 数据越界，原始大小{0} 剩余大小{1} 截取大小{2}", Bytes.Length, Remain, length));
            }
            byte[] bytes = new byte[length];
            Array.Copy(oBytes, offset, bytes, 0, length);
            // 所有读取数据均为小端模式, 如果是大端模式则需反转
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            Position = Position + length;
            return bytes;
        }
        #endregion
    }
}
