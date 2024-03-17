using System.Threading.Tasks;
using Zenject;

namespace ServerBrowser.Data.Discovery
{
    public class BssbApiServerDiscovery : ServerRepository.ServerDiscovery
    {
        [Inject] private readonly BssbApi _api = null!;
        
        public override async Task Refresh(ServerRepository repository)
        {
            var browseResponse = await _api.SendBrowseRequest();
            
            if (browseResponse != null)
            {
                foreach (var server in browseResponse.Lobbies)
                {
                    // TODO Rework this pretty much completely, just temporarily here to test the API
                    repository.DiscoverServer(new ServerRepository.ServerInfo()
                    {
                        Key = server.Key!,
                        ImageUrl = server.Level?.CoverArtUrl,
                        ServerName = server.Name!,
                        GameMode = server.GameModeDescription,
                        PlayerCount = server.ReadOnlyPlayerCount ?? 0,
                        PlayerLimit = server.PlayerLimit ?? 5,
                        ConnectionMethod = ServerRepository.ConnectionMethod.DirectConnect,
                        LobbyState = server.LobbyState ?? MultiplayerLobbyState.LobbySetup,
                        ServerCode = server.ServerCode,
                        ServerSecret = server.HostSecret
                    });
                }
            }
        }

        public override Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}