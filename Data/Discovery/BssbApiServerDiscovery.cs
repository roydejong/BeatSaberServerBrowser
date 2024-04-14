using System.Net;
using System.Threading.Tasks;
using BGNet.Core;
using ServerBrowser.UI.Browser;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Data.Discovery
{
    public class BssbApiServerDiscovery : ServerRepository.ServerDiscovery
    {
        [Inject] private readonly BssbApi _api = null!;
        
        private float? _lastRefreshTime = null;
        public const float RefreshInterval = 5f;
        
        public override async Task Refresh(ServerRepository repository)
        {
            if (_lastRefreshTime.HasValue && (Time.time - _lastRefreshTime.Value) < RefreshInterval)
                return;
            
            var browseResponse = await _api.SendBrowseRequest();
            _lastRefreshTime = Time.time;
            
            if (browseResponse != null)
            {
                foreach (var server in browseResponse.Lobbies)
                {
                    var connectionMethod = ServerRepository.ConnectionMethod.GameLiftModded;
                    IPEndPoint? dedicatedEndPoint = null;
                    BeatmapLevelSelectionMask? beatmapLevelSelectionMask = null;
                    GameplayServerConfiguration? gameplayServerConfiguration = null;

                    if (server.IsDirectConnect)
                    {
                        connectionMethod = ServerRepository.ConnectionMethod.DirectConnect;
                        dedicatedEndPoint = await server.EndPoint!.GetEndPointAsync(DefaultTaskUtility.instance);
                        beatmapLevelSelectionMask = BrowserFlowCoordinator.DefaultLevelSelectionMask;
                        gameplayServerConfiguration = new GameplayServerConfiguration(
                            server.PlayerLimit ?? 5,
                            DiscoveryPolicy.Public,
                            InvitePolicy.AnyoneCanInvite,
                            server.GameplayMode ?? GameplayServerMode.Managed,
                            server.SongSelectionMode,
                            GameplayServerControlSettings.All
                        );
                    }
                    else if (server.IsOfficial)
                    {
                        connectionMethod = ServerRepository.ConnectionMethod.GameLiftOfficial;
                        dedicatedEndPoint = null;
                    }
                    else if (server.MasterGraphUrl == null)
                    {
                        // Invalid server: not direct connect, not official, but also no master server? shouldn't happen
                        continue;
                    }
                    
                    repository.DiscoverServer(new ServerRepository.ServerInfo()
                    {
                        Key = server.Key!,
                        ImageUrl = server.Level?.CoverArtUrl,
                        ServerName = server.Name!,
                        GameModeName = server.GameModeDescription,
                        ServerTypeName = server.ServerTypeText,
                        PlayerCount = server.ReadOnlyPlayerCount ?? 0,
                        PlayerLimit = server.PlayerLimit ?? 5,
                        LobbyState = server.LobbyState ?? MultiplayerLobbyState.LobbySetup,
                        ConnectionMethod = connectionMethod,
                        ServerEndPoint = dedicatedEndPoint,
                        MasterServerGraphUrl = server.MasterGraphUrl,
                        MasterServerStatusUrl = server.MasterStatusUrl,
                        ServerCode = server.ServerCode,
                        ServerSecret = server.HostSecret,
                        ServerUserId = server.RemoteUserId,
                        BeatmapLevelSelectionMask = beatmapLevelSelectionMask,
                        GameplayServerConfiguration = gameplayServerConfiguration
                    });
                }
            }
        }

        public override Task Stop()
        {
            _lastRefreshTime = null;
            return Task.CompletedTask;
        }
    }
}