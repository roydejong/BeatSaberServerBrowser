using System.Threading.Tasks;
using ServerBrowser.Network.Discovery;
using Zenject;

namespace ServerBrowser.Data.Discovery
{
    public class LocalNetworkServerDiscovery : ServerRepository.ServerDiscovery
    {
        [Inject] private readonly DiscoveryClient _discoveryClient = null!;
        
        public override async Task Refresh(ServerRepository repository)
        {
            var isStarting = !_discoveryClient.IsActive;

            if (isStarting)
            {
                _discoveryClient.StartBroadcast();
                await Task.Delay(1); // wait briefly for any initial discovery responses so we can show them quickly
            }

            while (_discoveryClient.ReceivedResponses.Count > 0)
            {
                var response = _discoveryClient.ReceivedResponses.Dequeue();
                repository.DiscoverServer(new ServerRepository.ServerInfo()
                {
                    Key = response.ServerEndPoint.ToString(),
                    ImageUrl = null,
                    ServerName = response.ServerName,
                    GameModeName = response.GameModeName,
                    PlayerCount = response.PlayerCount,
                    PlayerLimit = response.GameplayServerConfiguration.maxPlayerCount,
                    LobbyState = MultiplayerLobbyState.None,
                    ConnectionMethod = ServerRepository.ConnectionMethod.DirectConnect,
                    ServerEndPoint = response.ServerEndPoint,
                    ServerCode = null,
                    ServerSecret = null,
                    ServerUserId = response.ServerUserId,
                    WasLocallyDiscovered = true
                });
            }
        }

        public override Task Stop()
        {
            if (_discoveryClient.IsActive)
                _discoveryClient.StopBroadcast();
            
            return Task.CompletedTask;
        }
    }
}