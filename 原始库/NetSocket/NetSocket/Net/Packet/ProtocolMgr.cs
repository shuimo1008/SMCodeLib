using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetSocket
{
    public class ProtocolMgr
    {
        private Dictionary<int, IPacket> PacketTable { get; set; }
        private Dictionary<int, IProtocolProcess> ProtocolTable { get; set; }

        private Queue<IPacket> Packets { get; set; }

        public ProtocolMgr()
        {
            PacketTable = new Dictionary<int, IPacket>();
            ProtocolTable = new Dictionary<int, IProtocolProcess>();
            Packets = new Queue<IPacket>();
            AssemblyPacket();
        }

        private void AssemblyPacket()
        {
            List<Type> typeList = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                typeList.AddRange(assemblies[i].GetTypes());
            }

            Type[] types = typeList.ToArray();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                object[] objs = type.GetCustomAttributes(typeof(ProtocolAttribute), false);
                if (objs.Length > 0)
                {
                    // 如果包含包属性,则创建对象
                    object obj = Activator.CreateInstance(type);
                    ProtocolAttribute oAttr = objs[0] as ProtocolAttribute;
                    if (oAttr.Type == ProcessType.Receive)
                    {
                        IPacket oPacket = obj as IPacket;
                        PacketTable.Add(oAttr.ID, oPacket);
                    }
                    else if (oAttr.Type == ProcessType.Process)
                    {
                        IProtocolProcess oProtocol =  obj as IProtocolProcess;
                        ProtocolTable.Add(oAttr.ID, oProtocol);
                    }
                }
            }
        }

        public int GetPacketID(IPacket oPacket)
        {
            if (oPacket.PacketID != 0) return oPacket.PacketID;
            else
            {
                object[] objs = GetType().GetCustomAttributes(typeof(ProtocolAttribute), false);
                if (objs.Length > 0)
                {
                    ProtocolAttribute oAttr = objs[0] as ProtocolAttribute;
                    oPacket.PacketID = oAttr.ID;
                }
                return oPacket.PacketID;
            }
        }

        public bool Process(AsyncSocketUserToken oUserToken, byte[] bytes, int offset, int count)
        {
            if (count < sizeof(int)) return false; // 判断包长度是否小于一个命令的长度

            int oPacketID = BitConverter.ToInt32(bytes, offset); // 获取命令ID
            IPacket oPacket = null;
            if (PacketTable.TryGetValue(oPacketID, out oPacket))
            {
                IPacketCreate oIPacket = oPacket as IPacketCreate;
                IPacket oNewPacket = oIPacket.New() as IPacket;
                try
                {
                    DynamicBuffer buffer = new DynamicBuffer(bytes, offset + sizeof(int), bytes.Length - offset - sizeof(int));
                    oNewPacket.Deserialization(buffer);
                    oNewPacket.PacketID = oPacketID;
                    oNewPacket.Owner = oUserToken;
                    Packets.Enqueue(oNewPacket);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void Tick()
        {
            while (Packets.Count > 0)
            {
                IPacket oPacket = Packets.Dequeue();
                IProtocolProcess protocolProcess = null;
                if (ProtocolTable.TryGetValue(oPacket.PacketID, out protocolProcess))
                {
                    try
                    {
                        protocolProcess.ToProcess(oPacket);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }


}
