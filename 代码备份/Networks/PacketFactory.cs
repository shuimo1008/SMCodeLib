using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZCSharpLib.Utils;

namespace ZCSharpLib.Networks
{
    public interface IPacketCreator
    {
        Packet CreatePacket();
    }

    public class PacketFactory
    {
        private Dictionary<int, IPacketCreator> CreatorTable { get; set; }

        public PacketFactory()
        {
            CreatorTable = new Dictionary<int, IPacketCreator>();

            Type[] oTypes = ReflUtils.GetAllTypes();

            foreach (var nType in oTypes)
            {
                if (!nType.IsClass) continue;

                Type oInterfaceType = nType.GetInterface(typeof(IPacketCreator).Name);

                if (oInterfaceType == typeof(IPacketCreator))
                {
                    PacketAttribute[] attributes = (PacketAttribute[])nType.GetCustomAttributes(typeof(PacketAttribute), false);

                    if (attributes != null && attributes.Length > 0)
                    {
                        PacketAttribute attribute = attributes[0];

                        if (CreatorTable.ContainsKey(attribute.PacketID))
                        {
                            App.Logger.Error("协议结构类已存在!PacketID：{0}", attribute.PacketID);
                            continue;
                        }

                        CreatorTable.Add(attribute.PacketID, (IPacketCreator)Activator.CreateInstance(nType));
                    }
                }
            }
        }

        public Packet CreatePacket(int iPacketID)
        {
            IPacketCreator iIPacketCreator = null;
            if (!CreatorTable.TryGetValue(iPacketID, out iIPacketCreator))
            {
                App.Logger.Error("没有找到对应的协议结构类!PacketID: {0}", iPacketID);
                return null;
            }
            return iIPacketCreator.CreatePacket();
        }
    }
}
