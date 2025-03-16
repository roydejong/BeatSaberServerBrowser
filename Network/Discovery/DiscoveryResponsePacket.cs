using System;
using System.Net;
using LiteNetLib.Utils;
using ServerBrowser.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

// ReSharper disable NotAccessedField.Global
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
        public int MaxPlayerCount;
        public int LobbyState;
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask;
        public GameplayServerConfiguration GameplayServerConfiguration;
        
        public void Serialize(NetDataWriter writer)
        {
            throw new NotImplementedException("Client does not write discovery response packets");
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
            MaxPlayerCount = reader.GetInt();
            LobbyState = reader.GetInt();
            BeatmapLevelSelectionMask = BeatmapLevelSelectionMask.Deserialize(reader);
            GameplayServerConfiguration = GameplayServerConfiguration.Deserialize(reader);
        }

        public BssbServer ToServerData()
        {
            return new BssbServer()
            {
                Key = "ld:" + ServerEndPoint.Address + ":" + ServerEndPoint.Port,
                RemoteUserId = ServerUserId,
                RemoteUserName = ServerName,
                EndPoint = new DnsEndPoint(ServerEndPoint),
                ReadOnlyPlayerCount = PlayerCount,
                PlayerLimit = MaxPlayerCount,
                LobbyState = (MultiplayerLobbyState)LobbyState,
                Name = ServerName,
                IsLocallyDiscovered = true,
                ServerTypeText = "Local server",
                MasterServerText = "Local server"
            };
        }
    }
}