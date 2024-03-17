using System;
using LiteNetLib.Utils;

namespace ServerBrowser.Network.Discovery
{
    public class DiscoveryQueryPacket : INetSerializable
    {
        public string Prefix;
        public int ProtocolVersion;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Prefix);
            writer.Put(ProtocolVersion);
        }

        public void Deserialize(NetDataReader reader)
        {
            throw new NotImplementedException("Client does not handle discovery packets");
        }
    }
}