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
        public DiscoveryPolicy DiscoveryPolicy;
        public InvitePolicy InvitePolicy;
        public int MaxPlayerCount;
        public GameplayServerConfiguration Configuration;
        public bool IsConnectionOwner;
        public bool IsDedicatedServer;
    }
}