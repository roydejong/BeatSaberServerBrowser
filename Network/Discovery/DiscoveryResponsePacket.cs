using System;
using System.Net;
using LiteNetLib.Utils;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace ServerBrowser.Network.Discovery
{
    public class DiscoveryResponsePacket : INetSerializable
    {
        public string Prefix;
        public int ProtocolVersion;
        
        public IPEndPoint ServerEndPoint;
        public string ServerName;
        public string ServerUserId;
        public string GameModeName;
        public int PlayerCount;
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask;
        public GameplayServerConfiguration GameplayServerConfiguration;
        
        public void Serialize(NetDataWriter writer)
        {
            throw new NotImplementedException("Client does not send discovery responses");
        }

        public void Deserialize(NetDataReader reader)
        {
            Prefix = reader.GetString();
            ProtocolVersion = reader.GetInt();
            
            ServerEndPoint = reader.GetNetEndPoint();
            ServerName = reader.GetString();
            ServerUserId = reader.GetString();
            GameModeName = reader.GetString();
            PlayerCount = reader.GetInt();
            BeatmapLevelSelectionMask = BeatmapLevelSelectionMask.Deserialize(reader);
            GameplayServerConfiguration = GameplayServerConfiguration.Deserialize(reader);
        }
    }
}