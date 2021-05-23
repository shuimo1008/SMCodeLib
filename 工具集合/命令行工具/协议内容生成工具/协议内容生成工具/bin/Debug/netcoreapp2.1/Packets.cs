using System;
using System.Collections.Generic;
using ZCSharpLib;
using ZCSharpLib.Nets;

#region 10001 协议内容 # 服务端发送会话ID
[Packet(10001)]
public class ResSessionPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResSessionPacket();
   }
}
/// <summary> 
/// # 服务端发送会话ID 
/// </summary> 
[Packet(10001)]
public class ResSessionPacket : Packet
{
   /// <summary> 
   ///  
   /// </summary> 
   public string SessionID {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10001;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(SessionID);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			SessionID = buffer.ReadUTF8();
       }
   }
}
#endregion

#region 10002 协议内容 # 客户端请求服务器时间
[Packet(10002)]
public class ReqServerTimePacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqServerTimePacket();
   }
}
/// <summary> 
/// # 客户端请求服务器时间 
/// </summary> 
[Packet(10002)]
public class ReqServerTimePacket : Packet
{
   /// <summary> 
   /// # 客户端时间 
   /// </summary> 
   public long Time {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10002;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteInt64(Time);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			Time = buffer.ReadInt64();
       }
   }
}
#endregion

#region 10003 协议内容 # 服务端响应客户端时间请求
[Packet(10003)]
public class ResServerTimePacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResServerTimePacket();
   }
}
/// <summary> 
/// # 服务端响应客户端时间请求 
/// </summary> 
[Packet(10003)]
public class ResServerTimePacket : Packet
{
   /// <summary> 
   /// # 客户端时间 
   /// </summary> 
   public long Time {get; set; } 
   /// <summary> 
   /// # 服务器时间 
   /// </summary> 
   public long ServerTime {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10003;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteInt64(Time);
			buffer.WriteInt64(ServerTime);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			Time = buffer.ReadInt64();
			ServerTime = buffer.ReadInt64();
       }
   }
}
#endregion

#region 10004 协议内容 # 客户端向服务端发送应用信息
[Packet(10004)]
public class ReqClientInfoPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqClientInfoPacket();
   }
}
/// <summary> 
/// # 客户端向服务端发送应用信息 
/// </summary> 
[Packet(10004)]
public class ReqClientInfoPacket : Packet
{
   /// <summary> 
   /// # 机器ID 
   /// </summary> 
   public int MachineID {get; set; } 
   /// <summary> 
   /// # 应用类型 服务程序=0;客户录制程序=1;客户控制程序=2 
   /// </summary> 
   public int ClientType {get; set; } 
   /// <summary> 
   /// # 应用场景 
   /// </summary> 
   public int ClientScene {get; set; } 
   /// <summary> 
   /// # 版本信息 
   /// </summary> 
   public string Version {get; set; } 
   /// <summary> 
   /// # 本地地址 
   /// </summary> 
   public string LocalIP {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10004;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteInt32(MachineID);
			buffer.WriteInt32(ClientType);
			buffer.WriteInt32(ClientScene);
			buffer.WriteUTF8(Version);
			buffer.WriteUTF8(LocalIP);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			MachineID = buffer.ReadInt32();
			ClientType = buffer.ReadInt32();
			ClientScene = buffer.ReadInt32();
			Version = buffer.ReadUTF8();
			LocalIP = buffer.ReadUTF8();
       }
   }
}
#endregion

#region 10005 协议内容 # 客户端N发送控制客户端M
[Packet(10005)]
public class ReqN2MPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqN2MPacket();
   }
}
/// <summary> 
/// # 客户端N发送控制客户端M 
/// </summary> 
[Packet(10005)]
public class ReqN2MPacket : Packet
{
   /// <summary> 
   /// # 发送的消息 
   /// </summary> 
   public string message {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10005;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(message);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			message = buffer.ReadUTF8();
       }
   }
}
#endregion

#region 10006 协议内容 # 客户端M发送控制客户端N
[Packet(10006)]
public class ReqM2NPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqM2NPacket();
   }
}
/// <summary> 
/// # 客户端M发送控制客户端N 
/// </summary> 
[Packet(10006)]
public class ReqM2NPacket : Packet
{
   /// <summary> 
   /// # 发送的消息 
   /// </summary> 
   public string message {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10006;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(message);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			message = buffer.ReadUTF8();
       }
   }
}
#endregion

#region 10007 协议内容 # 服务器发送用户信息
[Packet(10007)]
public class ResUsersPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResUsersPacket();
   }
}
/// <summary> 
/// # 服务器发送用户信息 
/// </summary> 
[Packet(10007)]
public class ResUsersPacket : Packet
{
   /// <summary> 
   /// # 用户列表 
   /// </summary> 
   public UserData[] Datas {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10007;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteInt32(Datas.Length);
			for(int i = 0; i < Datas.Length; i++)
			{
			    Datas[i].Serialization(buffer, isSerialize);
			}
       }
       else
       {
            PacketID = buffer.ReadInt32();
			int oDatasCount = buffer.ReadInt32();
			Datas = new UserData[oDatasCount];
			for(int i = 0; i < oDatasCount; i++)
			{
			    Datas[i] = new UserData();
			    Datas[i].Serialization(buffer, isSerialize);
			}
       }
   }

   /// <summary> 
   ///  
   /// </summary> 
   public class UserData
   {
       /// <summary> 
       /// # 用户Guid 
       /// </summary> 
       public string Guid {get; set; } 
       /// <summary> 
       /// # 用户名称 
       /// </summary> 
       public string UserName {get; set; } 

       public void Serialization(ByteBuffer buffer, bool isSerialize)
       {
           if (isSerialize)
           {
				buffer.WriteUTF8(Guid);
				buffer.WriteUTF8(UserName);
           }
           else
           {
				Guid = buffer.ReadUTF8();
				UserName = buffer.ReadUTF8();
           }
       }
   }
}
#endregion

#region 10008 协议内容 # 服务器发送用户工作信息
[Packet(10008)]
public class ResWorksPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResWorksPacket();
   }
}
/// <summary> 
/// # 服务器发送用户工作信息 
/// </summary> 
[Packet(10008)]
public class ResWorksPacket : Packet
{
   /// <summary> 
   /// # 工作数据数组 
   /// </summary> 
   public WorkData[] Datas {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10008;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteInt32(Datas.Length);
			for(int i = 0; i < Datas.Length; i++)
			{
			    Datas[i].Serialization(buffer, isSerialize);
			}
       }
       else
       {
            PacketID = buffer.ReadInt32();
			int oDatasCount = buffer.ReadInt32();
			Datas = new WorkData[oDatasCount];
			for(int i = 0; i < oDatasCount; i++)
			{
			    Datas[i] = new WorkData();
			    Datas[i].Serialization(buffer, isSerialize);
			}
       }
   }

   /// <summary> 
   /// # 工作数据 
   /// </summary> 
   public class WorkData
   {
       /// <summary> 
       /// # 工作数据GUID 
       /// </summary> 
       public string Guid {get; set; } 
       /// <summary> 
       /// # 工作数据所属用户 
       /// </summary> 
       public string UserGuid {get; set; } 
       /// <summary> 
       /// # 工作点数据组 
       /// </summary> 
       public WorkingpointData[] PointDatas {get; set; } 

       public void Serialization(ByteBuffer buffer, bool isSerialize)
       {
           if (isSerialize)
           {
				buffer.WriteUTF8(Guid);
				buffer.WriteUTF8(UserGuid);
				buffer.WriteInt32(PointDatas.Length);
				for(int i = 0; i < PointDatas.Length; i++)
				{
				    PointDatas[i].Serialization(buffer, isSerialize);
				}
           }
           else
           {
				Guid = buffer.ReadUTF8();
				UserGuid = buffer.ReadUTF8();
				int oPointDatasCount = buffer.ReadInt32();
				PointDatas = new WorkingpointData[oPointDatasCount];
				for(int i = 0; i < oPointDatasCount; i++)
				{
				    PointDatas[i] = new WorkingpointData();
				    PointDatas[i].Serialization(buffer, isSerialize);
				}
           }
       }
   }

   /// <summary> 
   /// # 工作点数据 
   /// </summary> 
   public class WorkingpointData
   {
       /// <summary> 
       /// # 工作点场景ID 
       /// </summary> 
       public int SceneGuid {get; set; } 
       /// <summary> 
       /// # 工作点状态:空置=0;开始=1;结束=2; 
       /// </summary> 
       public int OperateStatus {get; set; } 

       public void Serialization(ByteBuffer buffer, bool isSerialize)
       {
           if (isSerialize)
           {
				buffer.WriteInt32(SceneGuid);
				buffer.WriteInt32(OperateStatus);
           }
           else
           {
				SceneGuid = buffer.ReadInt32();
				OperateStatus = buffer.ReadInt32();
           }
       }
   }
}
#endregion

#region 10009 协议内容 # 客户端发送请求工作状态
[Packet(10009)]
public class ReqStartWorkPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqStartWorkPacket();
   }
}
/// <summary> 
/// # 客户端发送请求工作状态 
/// </summary> 
[Packet(10009)]
public class ReqStartWorkPacket : Packet
{
   /// <summary> 
   /// # 请求操作的用户ID 
   /// </summary> 
   public string UserGuid {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10009;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(UserGuid);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			UserGuid = buffer.ReadUTF8();
       }
   }
}
#endregion

#region 10010 协议内容 # 服务端发送响应工作状态请求
[Packet(10010)]
public class ResStartWorkPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResStartWorkPacket();
   }
}
/// <summary> 
/// # 服务端发送响应工作状态请求 
/// </summary> 
[Packet(10010)]
public class ResStartWorkPacket : Packet
{
   /// <summary> 
   /// # 返回状态: 成功=0;已经工作无法重复创建=1 
   /// </summary> 
   public int Status {get; set; } 
   /// <summary> 
   /// # 工作ID 
   /// </summary> 
   public string WorkGuid {get; set; } 
   /// <summary> 
   /// # 工作所属用户 
   /// </summary> 
   public string UserGuid {get; set; } 
   /// <summary> 
   /// # 工作点数据组 
   /// </summary> 
   public WorkingpointData[] PointDatas {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10010;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteInt32(Status);
			buffer.WriteUTF8(WorkGuid);
			buffer.WriteUTF8(UserGuid);
			buffer.WriteInt32(PointDatas.Length);
			for(int i = 0; i < PointDatas.Length; i++)
			{
			    PointDatas[i].Serialization(buffer, isSerialize);
			}
       }
       else
       {
            PacketID = buffer.ReadInt32();
			Status = buffer.ReadInt32();
			WorkGuid = buffer.ReadUTF8();
			UserGuid = buffer.ReadUTF8();
			int oPointDatasCount = buffer.ReadInt32();
			PointDatas = new WorkingpointData[oPointDatasCount];
			for(int i = 0; i < oPointDatasCount; i++)
			{
			    PointDatas[i] = new WorkingpointData();
			    PointDatas[i].Serialization(buffer, isSerialize);
			}
       }
   }

   /// <summary> 
   /// # 工作点数据 
   /// </summary> 
   public class WorkingpointData
   {
       /// <summary> 
       /// # 工作点场景ID 
       /// </summary> 
       public int SceneGuid {get; set; } 
       /// <summary> 
       /// # 工作点状态:空置=0;开始=1;结束=2; 
       /// </summary> 
       public int OperateStatus {get; set; } 

       public void Serialization(ByteBuffer buffer, bool isSerialize)
       {
           if (isSerialize)
           {
				buffer.WriteInt32(SceneGuid);
				buffer.WriteInt32(OperateStatus);
           }
           else
           {
				SceneGuid = buffer.ReadInt32();
				OperateStatus = buffer.ReadInt32();
           }
       }
   }
}
#endregion

#region 10011 协议内容 # 客户端发送结束工作请求
[Packet(10011)]
public class ReqEndWorkPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqEndWorkPacket();
   }
}
/// <summary> 
/// # 客户端发送结束工作请求 
/// </summary> 
[Packet(10011)]
public class ReqEndWorkPacket : Packet
{
   /// <summary> 
   /// # 工作ID 
   /// </summary> 
   public string WorkGuid {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10011;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(WorkGuid);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			WorkGuid = buffer.ReadUTF8();
       }
   }
}
#endregion

#region 10012 协议内容 # 服务端响应工作结束请求
[Packet(10012)]
public class ResEndWorkPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResEndWorkPacket();
   }
}
/// <summary> 
/// # 服务端响应工作结束请求 
/// </summary> 
[Packet(10012)]
public class ResEndWorkPacket : Packet
{
   /// <summary> 
   /// # 结束的工作ID 
   /// </summary> 
   public string WorkGuid {get; set; } 
   /// <summary> 
   /// # 状态: 成功结束=0; 工作占用中(失败)=1; 
   /// </summary> 
   public int Status {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10012;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(WorkGuid);
			buffer.WriteInt32(Status);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			WorkGuid = buffer.ReadUTF8();
			Status = buffer.ReadInt32();
       }
   }
}
#endregion

#region 10013 协议内容 # 客户端发送请求"修改工作状态"
[Packet(10013)]
public class ReqModifyWorkStatusPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqModifyWorkStatusPacket();
   }
}
/// <summary> 
/// # 客户端发送请求"修改工作状态" 
/// </summary> 
[Packet(10013)]
public class ReqModifyWorkStatusPacket : Packet
{
   /// <summary> 
   /// # 工作ID 
   /// </summary> 
   public string WorkGuid {get; set; } 
   /// <summary> 
   /// # 场景ID 
   /// </summary> 
   public int SceneGuid {get; set; } 
   /// <summary> 
   /// # 请求的操作状态: 开始=1;结束=2 
   /// </summary> 
   public int OperateStatusStatus {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10013;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(WorkGuid);
			buffer.WriteInt32(SceneGuid);
			buffer.WriteInt32(OperateStatusStatus);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			WorkGuid = buffer.ReadUTF8();
			SceneGuid = buffer.ReadInt32();
			OperateStatusStatus = buffer.ReadInt32();
       }
   }
}
#endregion

#region 10014 协议内容 # 服务端发送响应"修改工作状态"
[Packet(10014)]
public class ResModifyWorkStatusPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResModifyWorkStatusPacket();
   }
}
/// <summary> 
/// # 服务端发送响应"修改工作状态" 
/// </summary> 
[Packet(10014)]
public class ResModifyWorkStatusPacket : Packet
{
   /// <summary> 
   /// # 需要操作的工作ID 
   /// </summary> 
   public string WorkGuid {get; set; } 
   /// <summary> 
   /// # 场景ID 
   /// </summary> 
   public int SceneGuid {get; set; } 
   /// <summary> 
   /// # 请求的操作状态: 开始=1; 结束=2 
   /// </summary> 
   public int ReqOperateStatus {get; set; } 
   /// <summary> 
   /// # 响应的状态: 请求成功=0; 请求失败=1; 
   /// </summary> 
   public int ResOperateStatus {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10014;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(WorkGuid);
			buffer.WriteInt32(SceneGuid);
			buffer.WriteInt32(ReqOperateStatus);
			buffer.WriteInt32(ResOperateStatus);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			WorkGuid = buffer.ReadUTF8();
			SceneGuid = buffer.ReadInt32();
			ReqOperateStatus = buffer.ReadInt32();
			ResOperateStatus = buffer.ReadInt32();
       }
   }
}
#endregion

#region 10015 协议内容 # 客户端上传文件
[Packet(10015)]
public class ReqUploadFilePacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ReqUploadFilePacket();
   }
}
/// <summary> 
/// # 客户端上传文件 
/// </summary> 
[Packet(10015)]
public class ReqUploadFilePacket : Packet
{
   /// <summary> 
   /// # 用户ID 
   /// </summary> 
   public string UserGuid {get; set; } 
   /// <summary> 
   /// # 工作ID 
   /// </summary> 
   public string WorkGuid {get; set; } 
   /// <summary> 
   /// # 场景ID 
   /// </summary> 
   public int SceneGuid {get; set; } 
   /// <summary> 
   /// # 文件名称 
   /// </summary> 
   public string[] FileNames {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10015;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(UserGuid);
			buffer.WriteUTF8(WorkGuid);
			buffer.WriteInt32(SceneGuid);
			buffer.WriteInt32(FileNames.Length);
			for(int i = 0; i < FileNames.Length; i++)
			{
			    buffer.WriteUTF8(FileNames[i]);
			}
       }
       else
       {
            PacketID = buffer.ReadInt32();
			UserGuid = buffer.ReadUTF8();
			WorkGuid = buffer.ReadUTF8();
			SceneGuid = buffer.ReadInt32();
			int oFileNamesCount = buffer.ReadInt32();
			FileNames = new string[oFileNamesCount];
			for(int i = 0; i < oFileNamesCount; i++)
			{
			    FileNames[i] = buffer.ReadUTF8();
			}
       }
   }
}
#endregion

#region 10016 协议内容 
[Packet(10016)]
public class ResFullVideoToExitPacketCreator : IPacketCreator
{
   public IPacket CreatePacket()
   {
       return new ResFullVideoToExitPacket();
   }
}
/// <summary> 
///  
/// </summary> 
[Packet(10016)]
public class ResFullVideoToExitPacket : Packet
{
   /// <summary> 
   ///  
   /// </summary> 
   public string fileUrl {get; set; } 

   public override void Serialization(ByteBuffer buffer, bool isSerialize)
   {
       PacketID = 10016;
       if (isSerialize)
       {
            buffer.WriteInt32(PacketID);
			buffer.WriteUTF8(fileUrl);
       }
       else
       {
            PacketID = buffer.ReadInt32();
			fileUrl = buffer.ReadUTF8();
       }
   }
}
#endregion

