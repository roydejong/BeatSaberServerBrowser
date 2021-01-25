using ServerBrowser.Game;
using ServerBrowser.Harmony;
using ServerBrowser.UI.Components;
using ServerBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerBrowser.Core
{
    public static class GameStateManager
    {
        private static string _lobbyCode = null;
        private static string _customGameName = null;
        private static bool? _didSetLocalPlayerState = null;
        private static SemVer.Version _mpExVersion = null;

        private static bool _didAnnounce = false;
        private static bool _sentUnAnnounce = false;
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
            if (!MpLobbyConnectionTypePatch.IsPartyHost)
            {
                _lobbyCode = null;
                return;
            }

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

        public static void HandleUpdate()
        {
            var sessionManager = MpSession.SessionManager;

            if (sessionManager == null
                || !MpLobbyConnectionTypePatch.IsPartyMultiplayer
                || !MpLobbyConnectionTypePatch.IsPartyHost
                || !MpLobbyStatePatch.IsValidMpState)
            {
                // We are not in a party lobby, or we are not the host
                // Make sure any previous host announcements by us are cancelled and bail
                StatusText = "You must be the host of a custom multiplayer game.";
                HasErrored = true;

                UnAnnounce();

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            if (Plugin.Config.LobbyAnnounceToggle)
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

                UnAnnounce();

                if (!_didSetLocalPlayerState.HasValue || _didSetLocalPlayerState.Value == true)
                {
                    _didSetLocalPlayerState = false;
                    sessionManager.SetLocalPlayerState("lobbyannounce", false); // NB: this calls another update
                }

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            if (String.IsNullOrEmpty(_lobbyCode) || !sessionManager.isConnectionOwner
                || sessionManager.localPlayer == null || !sessionManager.isConnected
                || sessionManager.maxPlayerCount == 1)
            {
                // We do not (yet) have the Server Code, or we're at an in-between state where things aren't ready yet
                StatusText = "Can't send announcement (invalid lobby state).";
                HasErrored = true;

                UnAnnounce();

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            var lobbyAnnounce = GenerateAnnounce();

            StatusText = "Announcing your game to the world...\r\n" + lobbyAnnounce.Describe();
            HasErrored = false;

            LobbyConfigPanel.UpdatePanelInstance();

            DoAnnounce(lobbyAnnounce);
        }

        private static HostedGameData GenerateAnnounce()
        {
            var sessionManager = MpSession.SessionManager;
            var localPlayer = sessionManager.localPlayer;
            var connectedPlayers = sessionManager.connectedPlayers;

            if (_mpExVersion == null)
            {
                _mpExVersion = MpExHelper.GetInstalledVersion();
                
                if (_mpExVersion != null)
                {
                    Plugin.Log?.Info($"Detected MultiplayerExtensions, version {_mpExVersion}");
                }
            }

            var lobbyAnnounce = new HostedGameData()
            {
                ServerCode = _lobbyCode,
                GameName = MpSession.GetHostGameName(),
                OwnerId = localPlayer.userId,
                OwnerName = localPlayer.userName,
                PlayerCount = MpSession.GetPlayerCount(),
                PlayerLimit = MpSession.GetPlayerLimit(),
                IsModded = localPlayer.HasState("modded") || localPlayer.HasState("customsongs") || _mpExVersion != null,
                LobbyState = MpLobbyStatePatch.LobbyState,
                LevelId = _level?.levelID,
                SongName = _level?.songName,
                SongAuthor = _level?.songAuthorName,
                Difficulty = _difficulty,
                Platform = Plugin.PlatformId,
                MasterServerHost = MpConnect.LastUsedMasterServer != null ? MpConnect.LastUsedMasterServer.hostName : null,
                MasterServerPort = MpConnect.LastUsedMasterServer != null ? MpConnect.LastUsedMasterServer.port : MpConnect.DEFAULT_MASTER_PORT,
                MpExVersion = _mpExVersion
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

        private static async Task DoAnnounce(HostedGameData announce)
        {
            _sentUnAnnounce = false;

            if (await BSSBMasterAPI.Announce(announce))
            {
                _didAnnounce = true;
                _lastCompleteAnnounce = announce;

                StatusText = $"Players can now join from the browser!\r\n{announce.Describe()}";
                HasErrored = false;
            }
            else
            {
                _didAnnounce = false;
                _lastCompleteAnnounce = null;

                StatusText = $"Could not announce to master server!";
                HasErrored = true;
            }

            LobbyConfigPanel.UpdatePanelInstance();
        }

        /// <summary>
        /// Ensures that any host announcements made by us are removed:
        ///  - If a previous announcement was made, a request is sent to the master server, removing it.
        ///  - If no previous announcement was made, or it was already deleted, this is a no-op.
        /// </summary>
        public static async Task UnAnnounce()
        {
            if (_lastCompleteAnnounce != null && !_sentUnAnnounce)
            {
                _sentUnAnnounce = true;

                if (await BSSBMasterAPI.UnAnnounce(_lastCompleteAnnounce))
                {
                    Plugin.Log?.Info($"Host announcement was deleted OK!");

                    _didAnnounce = false;
                    _lastCompleteAnnounce = null;
                }
                else
                {
                    _sentUnAnnounce = false;
                }
            }
        }
    }
}
