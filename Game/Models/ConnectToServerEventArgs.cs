using System.Net;

namespace ServerBrowser.Game.Models
{
    public struct ConnectToServerEventArgs
    {
        public string UserId;
        public string UserName;
        public IPEndPoint RemoteEndPoint;
        public string Secret;
        public string Code;
        public BeatmapLevelSelectionMask SelectionMask;
        public GameplayServerConfiguration Configuration;
        public bool IsConnectionOwner;
        public bool IsDedicatedServer;
        public string ManagerId;
    }
}