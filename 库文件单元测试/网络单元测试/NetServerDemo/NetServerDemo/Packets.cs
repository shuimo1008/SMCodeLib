using System;
using System.Collections.Generic;
using ZCSharpLib;
using ZCSharpLib.Nets;

#region 10001 协议内容 # 客户端发送至服务端
[Packet(10001)]
public class Nc2sMessagePacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new Nc2sMessagePacket();
   }
}
/// <summary> 
/// # 客户端发送至服务端 
/// </summary> 
[Packet(10001)]
public class Nc2sMessagePacket : Packet
{
   /// <summary> 
   /// # 客户端ID 
   /// </summary> 
   public int ClientID {get; set; } 
   /// <summary> 
   /// # 消息ID 
   /// </summary> 
   public int MessageID {get; set; } 
   /// <summary> 
   /// # 发送时间 
   /// </summary> 
   public long SendTime {get; set; } 
   /// <summary> 
   /// # 消息 
   /// </summary> 
   public string Message {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10001;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteInt32(ClientID);
			buffer.WriteInt32(MessageID);
			buffer.WriteInt64(SendTime);
			buffer.WriteUTF8(Message);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			ClientID = buffer.ReadInt32();
			MessageID = buffer.ReadInt32();
			SendTime = buffer.ReadInt64();
			Message = buffer.ReadUTF8();
       }
   }
}
#endregion

#region 10002 协议内容 # 服务端发送至客户端
[Packet(10002)]
public class Ns2cMessagePacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new Ns2cMessagePacket();
   }
}
/// <summary> 
/// # 服务端发送至客户端 
/// </summary> 
[Packet(10002)]
public class Ns2cMessagePacket : Packet
{
   /// <summary> 
   /// # 客户端ID 
   /// </summary> 
   public int ClientID {get; set; } 
   /// <summary> 
   /// # 消息ID 
   /// </summary> 
   public int MessageID {get; set; } 
   /// <summary> 
   /// # 发送数据 
   /// </summary> 
   public long SendTime {get; set; } 
   /// <summary> 
   /// # 转发的消息 
   /// </summary> 
   public string message {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10002;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteInt32(ClientID);
			buffer.WriteInt32(MessageID);
			buffer.WriteInt64(SendTime);
			buffer.WriteUTF8(message);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			ClientID = buffer.ReadInt32();
			MessageID = buffer.ReadInt32();
			SendTime = buffer.ReadInt64();
			message = buffer.ReadUTF8();
       }
   }
}
#endregion

