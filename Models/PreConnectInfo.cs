using System.Net;

namespace ServerBrowser.Models
{
    public class PreConnectInfo
    {
        public string UserId;
        public string UserName;
        public IPEndPoint RemoteEndPoint;
        public string Secret;
        public string Code;
        public BeatmapLevelSelectionMask SelectionMask;
        public GameplayServerConfiguration Configuration;
        public byte[] PreMasterSecret;
        public byte[] MyRandom;
        public byte[] RemoteRandom;
        public bool IsConnectionOwner;
        public bool IsDedicatedServer;
        public string ManagerId;

        public PreConnectInfo(string userId, string userName, IPEndPoint remoteEndPoint, string secret, string code,
            BeatmapLevelSelectionMask selectionMask, GameplayServerConfiguration configuration, byte[] preMasterSecret,
            byte[] myRandom, byte[] remoteRandom, bool isConnectionOwner, bool isDedicatedServer, string managerId)
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