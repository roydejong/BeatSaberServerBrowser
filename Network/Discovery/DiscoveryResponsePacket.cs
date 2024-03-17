using System;
using System.Net;
using LiteNetLib.Utils;

namespace ServerBrowser.Network.Discovery
{
    public class DiscoveryResponsePacket : INetSerializable
    {
        public string Prefix;
        public int ProtocolVersion;
        public IPEndPoint ServerEndPoint;
        public int PlayerCount;
        public int PlayerLimit;
        public string ServerName;
        public string GameModeName;
        
        public void Serialize(NetDataWriter writer)
        {
            throw new NotImplementedException("Client does not send discovery responses");
        }

        public void Deserialize(NetDataReader reader)
        {
            Prefix = reader.GetString();
            ProtocolVersion = reader.GetInt();
            ServerEndPoint = reader.GetNetEndPoint();
            PlayerCount = reader.GetInt();
            PlayerLimit = reader.GetInt();
            ServerName = reader.GetString();
            GameModeName = reader.GetString();
        }
    }
}