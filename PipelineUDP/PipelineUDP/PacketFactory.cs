using PipelineUDP.Jsons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PipelineUDP
{
    public class PacketFactory
    {
        private Dictionary<string, Type> Packets { get; set; }

        public PacketFactory()
        {
            Packets = new Dictionary<string, Type>();

            List<Type> typeList = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                typeList.AddRange(assemblies[i].GetTypes());
            }
            Type[] oTypes = typeList.ToArray();

            for (int i = 0; i < oTypes.Length; i++)
            {
                Type oType = oTypes[i];
                if (oType.BaseType == typeof(Packet))
                {
                    object[] objs = oType.GetCustomAttributes(typeof(PacketAttribute), false);
                    if (objs.Length > 0)
                    {
                        PacketAttribute oAttr = objs[0] as PacketAttribute;
                        Packets.Add(oAttr.Token, oType);
                    }
                }
            }
        }

        public Packet Create(string token, string msg)
        {
            Type t = null;
            if (Packets.TryGetValue(token, out t))
            {
                return JsonUtility.Decode(msg, t) as Packet;
            }
            return null;
        }
    }
}
