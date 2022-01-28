using System.Net;

namespace ServerBrowser.Models
{
    public class PreConnectInfo
    {
        public readonly string UserId;
        public readonly string? UserName;
        public readonly IPEndPoint RemoteEndPoint;
        public readonly string Secret;
        public readonly string Code;
        public readonly BeatmapLevelSelectionMask SelectionMask;
        public readonly GameplayServerConfiguration Configuration;
        public readonly byte[]? PreMasterSecret;
        public readonly byte[]? MyRandom;
        public readonly byte[]? RemoteRandom;
        public readonly bool IsConnectionOwner;
        public readonly bool IsDedicatedServer;
        public readonly string? ManagerId;

        public PreConnectInfo(string userId, string? userName, IPEndPoint remoteEndPoint, string secret, string code,
            BeatmapLevelSelectionMask selectionMask, GameplayServerConfiguration configuration, byte[]? preMasterSecret,
            byte[]? myRandom, byte[]? remoteRandom, bool isConnectionOwner, bool isDedicatedServer, string? managerId)
        {
            UserId = userId;
            UserName = userName;
            RemoteEndPoint = remoteEndPoint;
            Secret = secret;
            Code = code;
            SelectionMask = selectionMask;
            Configuration = configuration;
            PreMasterSecret = preMasterSecret;
            MyRandom = myRandom;
            RemoteRandom = remoteRandom;
            IsConnectionOwner = isConnectionOwner;
            IsDedicatedServer = isDedicatedServer;
            ManagerId = managerId;
        }
    }
}