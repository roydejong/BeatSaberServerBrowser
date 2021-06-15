using ServerBrowser.Game;
using ServerBrowser.Harmony;
using ServerBrowser.UI.Components;
using ServerBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerBrowser.Core
{
    public static class GameStateManager
    {
        private static string _lobbyCode = null;
        private static string _hostSecret = null;
        private static bool _isDedicatedServer = false;
        private static string _customGameName = null;
        private static bool? _didSetLocalPlayerState = null;
        private static SemVer.Version _mpExVersion = null;

        private static bool _didAnnounce = false;
        private static HostedGameData _lastCompleteAnnounce = null;

        private static IPreviewBeatmapLevel _level = null;
        private static BeatmapDifficulty? _difficulty = null;

        public static string StatusText { get; private set; } = "Unknown status";
        public static bool HasErrored { get; private set; } = true;

        public static bool DidLeakCurrentCode
        {
            get
            {
                return _didAnnounce &&
                    _lastCompleteAnnounce != null
                    && !String.IsNullOrEmpty(_lobbyCode) 
                    && _lastCompleteAnnounce.ServerCode == _lobbyCode;
            }
        }

        #region Data Events
        public static void HandleSongSelected(IPreviewBeatmapLevel previewBeatmapLevel, BeatmapDifficulty beatmapDifficulty,
            BeatmapCharacteristicSO beatmapCharacteristic, GameplayModifiers gameplayModifiers)
        {
            if (!MpLobbyConnectionTypePatch.IsPartyHost)
                return;

            _level = previewBeatmapLevel;
            _difficulty = beatmapDifficulty;

            HandleUpdate();
        }

        public static void HandleLobbyCode(string lobbyCode)
        {
            if (_lobbyCode != lobbyCode)
            {
                _lobbyCode = lobbyCode;

                if (!String.IsNullOrEmpty(_lobbyCode))
                {
                    // Lobby code changed and isn't empty; force an update now
                    Plugin.Log?.Info($"Got lobby server code: \"{lobbyCode}\"");
                    HandleUpdate();
                }
            }
        }

        public static void HandleCustomGameName(string name)
        {
            if (!MpLobbyConnectionTypePatch.IsPartyHost)
                return;

            if (name != _customGameName)
            {
                Plugin.Log?.Info($"Got custom game name: \"{name}\"");

                _customGameName = name;
                HandleUpdate();
            }
        }

        public static void HandleConnectSuccess(string code, string secret, bool isDedicatedServer,
            GameplayServerConfiguration configuration)
        {
            Plugin.Log?.Info($"HandleConnectSuccess (code={code}, secret={secret}" +
                             $", isDedicatedServer={isDedicatedServer})");

            HandleLobbyCode(code);

            _isDedicatedServer = isDedicatedServer;
            _hostSecret = secret;

            if (_isDedicatedServer)
            {
                _difficulty = configuration.difficulties.FromMask();
            }
            
            HandleUpdate();
        }

        #endregion

        #region Update Action
        public static void HandleUpdate()
        {
            var sessionManager = MpSession.SessionManager;

            var isPartyMp = MpLobbyConnectionTypePatch.IsPartyMultiplayer && MpLobbyConnectionTypePatch.IsPartyHost;
            var isQuickplayMp = MpLobbyConnectionTypePatch.IsQuickplay;
            
            if (sessionManager == null || !MpLobbyStatePatch.IsValidMpState || (!isPartyMp && !isQuickplayMp))
            {
                // We are not in a party lobby, or we are not the host
                // Make sure any previous host announcements by us are cancelled and bail
                StatusText = "You must be the host of a custom multiplayer game.";
                HasErrored = true;

                _ = UnAnnounce();

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            if ((MpLobbyConnectionTypePatch.IsPartyHost && Plugin.Config.LobbyAnnounceToggle) ||
                (MpLobbyConnectionTypePatch.IsQuickplay && Plugin.Config.ShareQuickPlayGames))
            {
                // Toggle is on, ensure state is synced
                if (!_didSetLocalPlayerState.HasValue || _didSetLocalPlayerState.Value == false)
                {
                    _didSetLocalPlayerState = true;
                    sessionManager.SetLocalPlayerState("lobbyannounce", true); // NB: this calls another update
                }
            }
            else
            {
                // Toggle is off, ensure state is synced & do not proceed with announce
                StatusText = "Lobby announces are toggled off.";
                HasErrored = true;

                _ = UnAnnounce();

                if (!_didSetLocalPlayerState.HasValue || _didSetLocalPlayerState.Value == true)
                {
                    _didSetLocalPlayerState = false;
                    sessionManager.SetLocalPlayerState("lobbyannounce", false); // NB: this calls another update
                }

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            if ((String.IsNullOrEmpty(_lobbyCode) && String.IsNullOrEmpty(_hostSecret))
                || sessionManager.localPlayer == null
                || !sessionManager.isConnected
                || sessionManager.maxPlayerCount == 1)
            { 
                // We do not (yet) have the Server Code, or we're at an in-between state where things aren't ready yet
                StatusText = "Can't send announcement (invalid lobby state).";
                HasErrored = true;

                _ = UnAnnounce();

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            var lobbyAnnounce = GenerateAnnounce();

            StatusText = "Announcing your game to the world...\r\n" + lobbyAnnounce.Describe();
            HasErrored = false;

            LobbyConfigPanel.UpdatePanelInstance();

            _ = DoAnnounce(lobbyAnnounce);
        }

        private static HostedGameData GenerateAnnounce()
        {
            var sessionManager = MpSession.SessionManager;
            var localPlayer = sessionManager.localPlayer;
            var connectedPlayers = sessionManager.connectedPlayers;
            
            var serverType = HostedGameData.ServerTypePlayerHost;
            var connectionOwner = sessionManager.connectionOwner;

            if (connectionOwner.isMe)
            {
                if (_mpExVersion == null)
                {
                    _mpExVersion = MpExHelper.GetInstalledVersion();

                    if (_mpExVersion != null)
                    {
                        Plugin.Log?.Info($"Detected MultiplayerExtensions, version {_mpExVersion}");
                    }
                }
            }
            else
            {
                _mpExVersion = null;
            }

            if (MpLobbyConnectionTypePatch.IsQuickplay)
            {
                if (connectionOwner?.userName.StartsWith("BeatDedi/") ?? false)
                    serverType = HostedGameData.ServerTypeBeatDediQuickplay;
                else
                    serverType = HostedGameData.ServerTypeVanillaQuickplay;
            }
            
            Plugin.Log?.Info($"Server type determined as: {serverType}");

            var lobbyAnnounce = new HostedGameData()
            {
                ServerCode = _lobbyCode,
                GameName = MpSession.GetHostGameName(),
                OwnerId = connectionOwner.userId,
                OwnerName = connectionOwner.userName,
                PlayerCount = MpSession.GetPlayerCount(),
                PlayerLimit = MpSession.GetPlayerLimit(),
                IsModded = connectionOwner.HasState("modded") || connectionOwner.HasState("customsongs") || _mpExVersion != null,
                LobbyState = MpLobbyStatePatch.LobbyState,
                LevelId = _level?.levelID,
                SongName = _level?.songName,
                SongAuthor = _level?.songAuthorName,
                Difficulty = _difficulty,
                Platform = MpLocalPlayer.PlatformId,
                MasterServerHost = MpConnect.LastUsedMasterServer != null ? MpConnect.LastUsedMasterServer.hostName : null,
                MasterServerPort = MpConnect.LastUsedMasterServer != null ? MpConnect.LastUsedMasterServer.port : MpConnect.DEFAULT_MASTER_PORT,
                MpExVersion = _mpExVersion,
                ServerType = serverType,
                HostSecret = _hostSecret
            };

            lobbyAnnounce.Players = new List<HostedGamePlayer>();
            lobbyAnnounce.Players.Add(new HostedGamePlayer()
            {
                SortIndex = localPlayer.sortIndex,
                UserId = localPlayer.userId,
                UserName = localPlayer.userName,
                IsHost = localPlayer.isConnectionOwner,
                Latency = localPlayer.currentLatency
            });
            foreach (var connectedPlayer in connectedPlayers)
            {
                lobbyAnnounce.Players.Add(new HostedGamePlayer()
                {
                    SortIndex = connectedPlayer.sortIndex,
                    UserId = connectedPlayer.userId,
                    UserName = connectedPlayer.userName,
                    IsHost = connectedPlayer.isConnectionOwner,
                    Latency = connectedPlayer.currentLatency
                });
            }

            return lobbyAnnounce;
        }
        #endregion

        #region Announce/API
        private static Dictionary<string, AnnounceState> _announceStates = new();
        
        /// <summary>
        /// Sends a host announcement.
        /// </summary>
        private static async Task<bool> DoAnnounce(HostedGameData announce)
        {
            if (String.IsNullOrEmpty(announce.ServerCode))
                return false;
            
            // Get or initialize state
            AnnounceState announceState;

            if (!_announceStates.ContainsKey(announce.ServerCode))
            {
                _announceStates.Add(announce.ServerCode, new()
                {
                    ServerCode = announce.ServerCode,
                    OwnerId = announce.OwnerId,
                    HostSecret = announce.HostSecret
                });
            }

            announceState = _announceStates[announce.ServerCode];
            
            if (announceState.IsPending)
                return false;
            
            announceState.IsPending = true;

            // Try send announce
            var resultOk = false;
            
            if (await BSSBMasterAPI.Announce(announce))
            {
                announceState.IsPending = false;
                announceState.DidAnnounce = true;
                announceState.LastSuccess = DateTime.Now;

                StatusText = $"Players can now join from the browser!\r\n{announce.Describe()}";
                HasErrored = false;
            }
            else
            {
                announceState.IsPending = false;
                announceState.DidFail = true;
                announceState.LastFailure = DateTime.Now;

                StatusText = $"Could not announce to master server!";
                HasErrored = true;
            }

            LobbyConfigPanel.UpdatePanelInstance();
            return resultOk;
        }

        /// <summary>
        /// Ensures that any host announcements made by us previously are removed.
        /// </summary>
        public static async Task UnAnnounce()
        {
            foreach (var state in _announceStates.Values.ToArray())
            {
                if (await BSSBMasterAPI.UnAnnounce(state))
                {
                    _announceStates.Remove(state.ServerCode);
                }
            }
        }
        #endregion
    }
}
