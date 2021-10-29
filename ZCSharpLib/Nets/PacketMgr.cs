using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZCSharpLib.Times;
using ZCSharpLib.Utils;

namespace ZCSharpLib.Nets
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class PacketAttribute : Attribute
    {
        public int PacketID { get; private set; }
        public PacketAttribute(int packetID) { PacketID = packetID; }
    }

    public interface IPacket
    {
        int PacketID { get; }
        void SetSession(object session);
        void Serialization(ByteBuffer buffer, bool isSerialize);
    }

    public abstract class Packet : IPacket
    {
        public int PacketID { get; protected set; }
        public object Session { get; protected set; }

        public abstract void Serialization(ByteBuffer buffer, bool isSerialize);

        public void SetSession(object session) => Session = session;
    }


    public interface IProtocol
    {
        void ProcessAs(IPacket packet);
    }

    public abstract class Protocol<T> : IProtocol where T : IPacket
    {
        public void ProcessAs(IPacket packet)
        {
            Process((T)packet);
        }

        public abstract void Process(T packet);
    }

    public class PacketMgr
    {
        private PacketFactory PacketFactroy { get; set; }
        private ConcurrentQueue<IPacket> Packets { get; set; }

        private Dictionary<int, IProtocol> Processers { get; set; }

        public PacketMgr()
        {
            Packets = new ConcurrentQueue<IPacket>();
            PacketFactroy = new PacketFactory();
            Processers = new Dictionary<int, IProtocol>();
            AssemblyProcesser();
            App.SubscribeUpdate(Update);
        }

        private void AssemblyProcesser()
        {
            Type[] oTypes = ReflUtils.GetAllTypes();
            foreach (var type in oTypes)
            {
                if (!type.IsClass) continue;
                if (type.BaseType == null) continue;
                if (type.BaseType.Name == typeof(Protocol<>).Name)
                {
                    Type[] genericTypes = type.BaseType.GenericTypeArguments;
                    if (genericTypes.Length > 0)
                    {
                        Type packetType = genericTypes[0];
                        PacketAttribute[] attributes = (PacketAttribute[])packetType.GetCustomAttributes(typeof(PacketAttribute), false);
                        if (attributes.Length > 0)
                        {
                            PacketAttribute attr = attributes[0];
                            if (Processers.ContainsKey(attr.PacketID))
                            {
                                App.Error($"协议结构处理类已存在!PacketID：{attr.PacketID}");
                                continue;
                            }
                            Processers.Add(attr.PacketID, (IProtocol)Activator.CreateInstance(type));
                        }
                    }
                }
            }
        }

        public IPacket CreatePacket(int packetID)
        {
            return PacketFactroy.CreatePacket(packetID);
        }

        public void Push(IPacket packet)
        {
            Packets.Enqueue(packet);
        }

        public void Update(float deltaTime)
        {
            while (Packets.Count > 0)
            {
                IPacket oPacket = null;
                if (Packets.TryDequeue(out oPacket))
                {
                    IProtocol oProcess = null;
                    if (Processers.TryGetValue(oPacket.PacketID, out oProcess))
                    {
                        try
                        {
                            oProcess.ProcessAs(oPacket);
                        }
                        catch (Exception e)
                        {
                            App.Error($"协议调用堆栈错误!:\n{e}");
                        }
                    }
                    //else
                    //{
                    //    App.Error("PotocolID={0}没有找到协议处理类:", oPacket.PacketID);
                    //}
                }
            }
        }
    }
}
