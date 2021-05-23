using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZCSharpLib.Utils;

namespace ZCSharpLib.Nets
{
    public interface IPacketCreator
    {
        IPacket CreatePacket();
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
                            App.Error("协议结构类已存在!PacketID：{0}", attribute.PacketID);
                            continue;
                        }

                        CreatorTable.Add(attribute.PacketID, (IPacketCreator)Activator.CreateInstance(nType));
                    }
                }
            }
        }

        public IPacket CreatePacket(int packetID)
        {
            IPacketCreator packetCreator = null;
            if (!CreatorTable.TryGetValue(packetID, out packetCreator))
            {
                App.Error("没有找到对应的协议结构类!PacketID: {0}", packetID);
                return null;
            }
            return packetCreator.CreatePacket();
        }
    }
}
