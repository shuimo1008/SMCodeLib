using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PipelineUDP
{
    public interface IPacketProcesser
    {
        void Process(Packet packet);
    }

    public abstract class PacketProcesser<T> : IPacketProcesser where T : Packet
    {
        public void Process(Packet packet)
        {
            T t = (T)packet;
            if (t != null) Process(t);
        }

        public abstract void Process(T packet);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PacketAttribute : Attribute
    {
        public string Token { get; set; }
    }

    public class PacketProcess
    {
        private Dictionary<string, IPacketProcesser> Processers { get; set; }

        public PacketProcess()
        {
            Processers = new Dictionary<string, IPacketProcesser>();

            List<Type> typeList = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                typeList.AddRange(assemblies[i].GetTypes());
            }
            Type[] oTypes = typeList.ToArray();

            for (int i = 0; i < oTypes.Length; i++)
            {
                Type nType = oTypes[i];
                Type oInterfaceType = nType.GetInterface(typeof(IPacketProcesser).Name);

                if (nType.BaseType != null && nType.BaseType.Name == typeof(PacketProcesser<>).Name)
                {
                    PacketAttribute[] attributes = (PacketAttribute[])nType.GetCustomAttributes(typeof(PacketAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        PacketAttribute attr = attributes[0];
                        if (Processers.ContainsKey(attr.Token))
                        {
                            throw new Exception(string.Format("协议结构处理类已存在!Token：{0}", attr.Token));
                        }
                        Processers.Add(attr.Token, (IPacketProcesser)Activator.CreateInstance(nType));
                    }
                }
            }
        }

        public void Process(Packet packet)
        {
            IPacketProcesser processer = null;
            if (Processers.TryGetValue(packet.token, out processer))
            {
                processer.Process(packet);
            }
        }
    }
}
