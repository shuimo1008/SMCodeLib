using System;
using System.Collections.Generic;
using ZCSharpLib;
using ZCSharpLib.Nets;

#region 10001 用户登录
[Packet(10001)]
public class ReqUserLogin1PacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqUserLogin1Packet();
   }
}
/// <summary> 
///  
/// </summary> 
[Packet(10001)]
public class ReqUserLogin1Packet : IPacket
{
   /// <summary> 
   /// # 用户名 
   /// </summary> 
   public string UserName {get; set; } 
   /// <summary> 
   /// # 用户密码 
   /// </summary> 
   public string UserPass {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public byte[] Test1 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public bool[] Test2 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public short[] Test3 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public ushort[] test4 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public int[] test5 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public uint[] test6 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public long[] test7 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public ulong[] test8 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public float[] test9 {get; set; } 
   /// <summary> 
   ///  
   /// </summary> 
   public string[] Test10 {get; set; } 
   /// <summary> 
   /// # 用户身体信息 
   /// </summary> 
   public UserBodyInfo UserBody {get; set; } 

   public void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10001;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			UserName = buffer.ReadUTF8();
			UserPass = buffer.ReadUTF8();
			int oTest1Count = buffer.ReadInt32();
			Test1 = buffer.ReadBytes(oTest1Count);
			int oTest2Count = buffer.ReadInt32();
			Test2 = new bool[oTest2Count];
			for(int i = 0; i < oTest2Count; i++)
			{
			    Test2[i] = buffer.ReadBool();
			}
			int oTest3Count = buffer.ReadInt32();
			Test3 = new short[oTest3Count];
			for(int i = 0; i < oTest3Count; i++)
			{
			    Test3[i] = buffer.ReadInt16();
			}
			int otest4Count = buffer.ReadInt32();
			test4 = new ushort[otest4Count];
			for(int i = 0; i < otest4Count; i++)
			{
			    test4[i] = buffer.ReadUInt16();
			}
			int otest5Count = buffer.ReadInt32();
			test5 = new int[otest5Count];
			for(int i = 0; i < otest5Count; i++)
			{
			    test5[i] = buffer.ReadInt32();
			}
			int otest6Count = buffer.ReadInt32();
			test6 = new uint[otest6Count];
			for(int i = 0; i < otest6Count; i++)
			{
			    test6[i] = buffer.ReadUInt32();
			}
			int otest7Count = buffer.ReadInt32();
			test7 = new long[otest7Count];
			for(int i = 0; i < otest7Count; i++)
			{
			    test7[i] = buffer.ReadInt64();
			}
			int otest8Count = buffer.ReadInt32();
			test8 = new ulong[otest8Count];
			for(int i = 0; i < otest8Count; i++)
			{
			    test8[i] = buffer.ReadUInt64();
			}
			int otest9Count = buffer.ReadInt32();
			test9 = new float[otest9Count];
			for(int i = 0; i < otest9Count; i++)
			{
			    test9[i] = buffer.ReadFloat();
			}
			int oTest10Count = buffer.ReadInt32();
			Test10 = new string[oTest10Count];
			for(int i = 0; i < oTest10Count; i++)
			{
			    Test10[i] = buffer.ReadUTF8();
			}
			UserBody.Serialization(buffer, isSerialize);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			buffer.WriteUTF8(UserName);
			buffer.WriteUTF8(UserPass);
			buffer.WriteInt32(Test1.Length);
			buffer.WriteBytes(Test1);
			buffer.WriteInt32(Test2.Length);
			for(int i = 0; i < Test2.Length; i++)
			{
			    buffer.WriteBool(Test2[i]);
			}
			buffer.WriteInt32(Test3.Length);
			for(int i = 0; i < Test3.Length; i++)
			{
			    buffer.WriteInt16(Test3[i]);
			}
			buffer.WriteInt32(test4.Length);
			for(int i = 0; i < test4.Length; i++)
			{
			    buffer.WriteUInt16(test4[i]);
			}
			buffer.WriteInt32(test5.Length);
			for(int i = 0; i < test5.Length; i++)
			{
			    buffer.WriteInt32(test5[i]);
			}
			buffer.WriteInt32(test6.Length);
			for(int i = 0; i < test6.Length; i++)
			{
			    buffer.WriteUInt32(test6[i]);
			}
			buffer.WriteInt32(test7.Length);
			for(int i = 0; i < test7.Length; i++)
			{
			    buffer.WriteInt64(test7[i]);
			}
			buffer.WriteInt32(test8.Length);
			for(int i = 0; i < test8.Length; i++)
			{
			    buffer.WriteUInt64(test8[i]);
			}
			buffer.WriteInt32(test9.Length);
			for(int i = 0; i < test9.Length; i++)
			{
			    buffer.WriteFloat(test9[i]);
			}
			buffer.WriteInt32(Test10.Length);
			for(int i = 0; i < Test10.Length; i++)
			{
			    buffer.WriteUTF8(Test10[i]);
			}
			UserBody.Serialization(buffer, isSerialize);
       }
   }
   public int PacketID { get; set; }
   public object Token { get; set; }
   public object Owner { get; set; }

   /// <summary> 
   ///  
   /// </summary> 
   public struct UserBodyInfo
   {
       /// <summary> 
       /// # 指纹ID 
       /// </summary> 
       public int Figer {get; set; } 
       /// <summary> 
       /// # 眼睛ID 
       /// </summary> 
       public int Eye {get; set; } 

       public void Serialization(ByteBuffer buffer, bool isSerialize)
       {
           if (isSerialize)
           {
				Figer = buffer.ReadInt32();
				Eye = buffer.ReadInt32();
           }
           else
           {
				buffer.WriteInt32(Figer);
				buffer.WriteInt32(Eye);
           }
       }
   }

   /// <summary> 
   ///  
   /// </summary> 
   public struct UserTest
   {
       /// <summary> 
       ///  
       /// </summary> 
       public UserBodyInfo info {get; set; } 

       public void Serialization(ByteBuffer buffer, bool isSerialize)
       {
           if (isSerialize)
           {
				info.Serialization(buffer, isSerialize);
           }
           else
           {
				info.Serialization(buffer, isSerialize);
           }
       }
   }
}
#endregion

#region 10002 用户登录
[Packet(10002)]
public class ResUserLogin1PacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResUserLogin1Packet();
   }
}
/// <summary> 
/// # 用户登录响应 
/// </summary> 
[Packet(10002)]
public class ResUserLogin1Packet : IPacket
{
   /// <summary> 
   ///  
   /// </summary> 
   public int Status {get; set; } 

   public void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10002;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			Status = buffer.ReadInt32();
       }
       else
       {
            PacketID = buffer.ReadInt32();
			buffer.WriteInt32(Status);
       }
   }
   public int PacketID { get; set; }
   public object Token { get; set; }
   public object Owner { get; set; }
}
#endregion

