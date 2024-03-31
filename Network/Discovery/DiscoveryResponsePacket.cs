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
        public string ServerTypeName;
        public int PlayerCount;
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask;
        public GameplayServerConfiguration GameplayServerConfiguration;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Prefix);
            writer.Put(ProtocolVersion);
            
            writer.Put(ServerEndPoint);
            writer.Put(ServerName);
            writer.Put(ServerUserId);
            writer.Put(GameModeName);
            writer.Put(ServerTypeName);
            writer.Put(PlayerCount);
            BeatmapLevelSelectionMask.Serialize(writer);
            GameplayServerConfiguration.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            Prefix = reader.GetString();
            ProtocolVersion = reader.GetInt();
            
            ServerEndPoint = reader.GetNetEndPoint();
            ServerName = reader.GetString();
            ServerUserId = reader.GetString();
            GameModeName = reader.GetString();
            ServerTypeName = reader.GetString();
            PlayerCount = reader.GetInt();
            BeatmapLevelSelectionMask = BeatmapLevelSelectionMask.Deserialize(reader);
            GameplayServerConfiguration = GameplayServerConfiguration.Deserialize(reader);
        }
    }
}