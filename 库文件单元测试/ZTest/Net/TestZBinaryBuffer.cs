using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZCSharpLib.Net;

namespace ZTest
{
    public class TestZBinaryBuffer
    {
        public static void Start()
        {
            Student oStudent = new Student();

            DynamicBuffer oByteArray = new DynamicBuffer();
            oByteArray.WriteByte(1);
            oByteArray.WriteUTF8("赵燕秋");
            oByteArray.WriteInt16(4);
            oByteArray.WriteUInt32(12);
            oByteArray.WriteUInt64(78);
            oByteArray.WriteFloat(15.6f);
            oByteArray.WriteDouble(90.123111);
            oByteArray.WriteInt32(2);
            oByteArray.WriteUTF8("你好世界!");
            byte[] bytes = Encoding.UTF8.GetBytes("冬去春来！");
            byte[] lenghtBytes = BitConverter.GetBytes(bytes.Length);
            oByteArray.WriteBytes(lenghtBytes, 0, lenghtBytes.Length);
            oByteArray.WriteBytes(bytes, 0, bytes.Length);

            DynamicBuffer oTmpArray = new DynamicBuffer(oByteArray.Buffer);
            Console.WriteLine(oTmpArray.ReadByte());
            Console.WriteLine(oTmpArray.ReadUTF8());
            Console.WriteLine(oTmpArray.ReadInt16());
            Console.WriteLine(oTmpArray.ReadUInt32());
            Console.WriteLine(oTmpArray.ReadUInt64());
            Console.WriteLine(oTmpArray.ReadFloat());
            Console.WriteLine(oTmpArray.ReadDouble());
            Console.WriteLine(oTmpArray.ReadInt32());
            Console.WriteLine(oTmpArray.ReadUTF8());
            Console.WriteLine(oTmpArray.ReadUTF8());
            Console.ReadKey();
        }
    }

    public class Student
    {
        public bool mBool;
        public byte mByte;
        public int mInt;
        public long mLong;
        public uint mUInt;
        public ulong mULong;
        public float mFloat;
        public double mDouble;
        public string mString;
    }
}
