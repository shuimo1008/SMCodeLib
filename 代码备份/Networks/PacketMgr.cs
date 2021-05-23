using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZCSharpLib.Utils;

namespace ZCSharpLib.Networks
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PacketAttribute : Attribute
    {
        public int PacketID { get; private set; }
        public PacketAttribute(int iPacketID) { PacketID = iPacketID; }
    }

    public interface IPacketProcesser
    {
        void Process(Packet iPacket);
    }

    public abstract class ProtocolProcesser<T> : IPacketProcesser where T : Packet
    {
        public void Process(Packet iPacket)
        {
            Process(iPacket as T);
        }

        public abstract void Process(T iPacket);
    }

    public class PacketMgr
    {
        private Queue<Packet> PacketQueue { get; set; }

        private Dictionary<int, IPacketProcesser> Processers { get; set; }

        public PacketMgr()
        {
            PacketQueue = new Queue<Packet>();
            Processers = new Dictionary<int, IPacketProcesser>();
            AssemblyProcesser();
        }

        private void AssemblyProcesser()
        {
            Type[] oTypes = ReflUtils.GetAllTypes();
            foreach (var nType in oTypes)
            {
                if (!nType.IsClass) continue;
                Type oInterfaceType = nType.GetInterface(typeof(IPacketProcesser).Name);
                if (oInterfaceType == typeof(IPacketProcesser))
                {
                    PacketAttribute[] attributes = (PacketAttribute[])nType.GetCustomAttributes(typeof(PacketAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        PacketAttribute attr = attributes[0];
                        if (Processers.ContainsKey(attr.PacketID))
                        {
                            App.Logger.Error("协议结构处理类已存在!PacketID：{0}", attr.PacketID);
                            continue;
                        }
                        Processers.Add(attr.PacketID, (IPacketProcesser)Activator.CreateInstance(nType));
                    }
                }
            }
        }

        private void Process(Packet iPacket)
        {
            IPacketProcesser oProcess = null;
            if (Processers.TryGetValue(iPacket.PacketID, out oProcess))
            {
                oProcess.Process(iPacket);
            }
            else
            {
                App.Logger.Error("PotocolID={0}没有找到协议处理类:", iPacket.PacketID);
            }
        }

        public void PacketProcess(Packet iPacket)
        {
            lock (PacketQueue)
            {
                PacketQueue.Enqueue(iPacket);
            }
        }

        public void Update()
        {
            lock (PacketQueue)
            {
                while (PacketQueue.Count > 0)
                {
                    Packet oPacket = PacketQueue.Dequeue();
                    Process(oPacket);
                }
            }
        }
    }
}
