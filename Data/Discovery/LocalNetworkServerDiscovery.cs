using System.Threading.Tasks;
using ServerBrowser.Network.Discovery;
using Zenject;

namespace ServerBrowser.Data.Discovery
{
    public class LocalNetworkServerDiscovery : ServerRepository.ServerDiscovery
    {
        [Inject] private readonly DiscoveryClient _discoveryClient = null!;
        
        public override Task Refresh(ServerRepository repository)
        {
            if (!_discoveryClient.IsActive)
                _discoveryClient.StartBroadcast();
            
            while (_discoveryClient.ReceivedResponses.Count > 0)
            {
                var response = _discoveryClient.ReceivedResponses.Dequeue();
                repository.DiscoverServer(new ServerRepository.ServerInfo()
                {
                    Key = response.ServerEndPoint.ToString(),
                    ServerName = response.ServerName,
                    GameMode = response.GameModeName,
                    PlayerCount = response.PlayerCount,
                    PlayerLimit = response.PlayerLimit,
                    WasLocallyDiscovered = true
                });
            }
            
            return Task.CompletedTask;
        }

        public override Task Stop()
        {
            if (_discoveryClient.IsActive)
                _discoveryClient.StopBroadcast();
            
            return Task.CompletedTask;
        }
    }
}