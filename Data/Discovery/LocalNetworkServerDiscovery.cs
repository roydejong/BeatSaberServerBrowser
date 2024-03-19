using System.Collections.Generic;
using System.Threading.Tasks;
using ServerBrowser.Network.Discovery;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Data.Discovery
{
    public class LocalNetworkServerDiscovery : ServerRepository.ServerDiscovery
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly DiscoveryClient _discoveryClient = null!;
        
        private readonly List<string> _endpointsSeen = new();
        
        public override Task Refresh(ServerRepository repository)
        {
            if (!_discoveryClient.IsActive)
                _discoveryClient.StartBroadcast();

            while (_discoveryClient.ReceivedResponses.Count > 0)
            {
                var response = _discoveryClient.ReceivedResponses.Dequeue();

                var endPointStr = response.ServerEndPoint.ToString();
                if (!_endpointsSeen.Contains(endPointStr))
                {
                    _log.Info($"Discovered local network server: {endPointStr}, {response.ServerName}");
                    _endpointsSeen.Add(endPointStr);
                }
                
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
                    WasLocallyDiscovered = true,
                    BeatmapLevelSelectionMask = response.BeatmapLevelSelectionMask,
                    GameplayServerConfiguration = response.GameplayServerConfiguration
                });
            }
            
            return Task.CompletedTask;
        }

        public override Task Stop()
        {
            if (_discoveryClient.IsActive)
                _discoveryClient.StopBroadcast();
            
            _endpointsSeen.Clear();
            
            return Task.CompletedTask;
        }
    }
}