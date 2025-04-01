using System;
using LiteNetLib.Utils;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace ServerBrowser.Network.Discovery
{
    public class DiscoveryQueryPacket : INetSerializable
    {
        public string Prefix;
        public int ProtocolVersion;
        
        public string GameVersion;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Prefix);
            writer.Put(ProtocolVersion);
            
            writer.Put(GameVersion);
        }

        public void Deserialize(NetDataReader reader)
        {
            throw new NotImplementedException("Client does not read discovery query packets");
        }
    }
}