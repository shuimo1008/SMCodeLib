
namespace NetSocket
{
    public abstract class BasePacket<T> : IPacket, IPacketCreate where T : IPacket, new()
    {
        public int PacketID { get; set; }
        public object Owner { get; set; }
        public virtual void Deserialization(DynamicBuffer oBuffer) { }
        public virtual void Serialization(DynamicBuffer oBuffer) { }
        public virtual object New()
        {
            return new T();
        }
    }
}
