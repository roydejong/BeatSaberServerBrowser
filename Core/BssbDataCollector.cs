using System;
using System.Linq;
using System.Net;
using ServerBrowser.Models;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Collects server and lobby data from the multiplayer session, so it can be relayed to the server browser.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbDataCollector : IInitializable, IDisposable, IAffinity
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly IMultiplayerSessionManager _multiplayerSession = null!;
        [Inject] private readonly ServerBrowserClient _serverBrowserClient = null!;

        public bool SessionActive { get; private set; }
        public BssbServerDetail Current { get; private set; } = new();
        public PreConnectInfo? PreConnectInfo { get; private set; } = null;

        public event EventHandler? DataChanged;
        public event EventHandler<BssbServerDetail>? SessionEstablished;
        public event EventHandler? SessionEnded;

        public void Initialize()
        {
            _multiplayerSession.connectedEvent += HandleSessionConnected;
            _multiplayerSession.disconnectedEvent += HandleSessionDisconnected;
            _multiplayerSession.playerConnectedEvent += HandlePlayerConnected;
            _multiplayerSession.playerDisconnectedEvent += HandlePlayerDisconnected;
        }

        public void Dispose()
        {
            _multiplayerSession.connectedEvent -= HandleSessionConnected;
            _multiplayerSession.disconnectedEvent -= HandleSessionDisconnected;
            _multiplayerSession.playerConnectedEvent -= HandlePlayerConnected;
            _multiplayerSession.playerDisconnectedEvent -= HandlePlayerDisconnected;
        }
        
        internal void TriggerDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private void HandleSessionConnected()
        {
            _log.Info($"Multiplayer session connected (syncTime={_multiplayerSession.syncTime})");

            SessionActive = true;

            if (IsPartyLeader)
                Current.Name = _serverBrowserClient.PreferredServerName;

            HandlePlayerConnected(_multiplayerSession.connectionOwner);
            HandlePlayerConnected(_multiplayerSession.localPlayer);

            if (Current.IsBeatTogetherHost)
            {
                _log.Info("Detected a BeatTogether host");
            }
            
            Current.ServerTypeCode = DetermineServerType();

            SessionEstablished?.Invoke(this, Current);
        }

        private void HandleSessionDisconnected(DisconnectedReason reason)
        {
            if (!SessionActive)
                return;
            
            _log.Info($"Multiplayer session disconnected (reason={reason})");

            SessionActive = false;

            SessionEnded?.Invoke(this, EventArgs.Empty);
        }

        private void HandlePlayerConnected(IConnectedPlayer player)
        {
            if (ContainsPlayer(player.userId))
                return;

            _log.Info($"Player connected to session (sortIndex={player.sortIndex}, userId={player.userId}, "
                      + $"userName={player.userName}, isMe={player.isMe}, isConnectionOwner={player.isConnectionOwner}, "
                      + $"currentLatency={player.currentLatency})");

            var bssbServerPlayer = BssbServerPlayer.FromConnectedPlayer(player);
            bssbServerPlayer.IsPartyLeader = (Current.ManagerId == player.userId);
            Current.Players.Add(bssbServerPlayer);

            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandlePlayerDisconnected(IConnectedPlayer player)
        {
            _log.Info($"Player disconnected from session (userId={player.userId}, userName={player.userName})");

            var playerToRemove = Current.Players.FirstOrDefault(p => p.UserId == player.userId);

            if (playerToRemove == null)
                return;
            
            Current.Players.Remove(playerToRemove);
                
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private void HandleLobbyStateChanged(MultiplayerLobbyState newState)
        {
            if (Current.LobbyState == newState)
                return;

            _log.Info($"Lobby state changed to: {newState}");
            
            Current.LobbyState = newState;
            
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private string DetermineServerType()
        {
            if (Current.IsOfficial)
                return Current.IsQuickPlay ? "vanilla_quickplay" : "vanilla_dedicated";
            
            if (Current.IsBeatTogetherHost)
                return Current.IsQuickPlay ? "beattogether_quickplay" : "beattogether_dedicated";
            
            return "unknown";
        }
        
        public bool ContainsPlayer(string userId) => Current.Players.Any(p => p.UserId == userId);

        public bool IsPartyLeader =>
            _multiplayerSession.localPlayer.userId == Current.ManagerId;

        [AffinityPostfix]
        [AffinityPatch(typeof(MasterServerConnectionManager), "HandleConnectToServerSuccess")]
        private void HandlePreConnect(string userId, string userName, IPEndPoint remoteEndPoint, string secret,
            string code, BeatmapLevelSelectionMask selectionMask, GameplayServerConfiguration configuration,
            byte[] preMasterSecret, byte[] myRandom, byte[] remoteRandom, bool isConnectionOwner,
            bool isDedicatedServer, string managerId)
        {
            // nb: HandleConnectToServerSuccess just means "the master server gave us the info to connect"
            _log.Info($"Game will connect to server (userId={userId}, userName={userName}, "
                      + $"remoteEndPoint={remoteEndPoint}, secret={secret}, code={code}, "
                      + $"isDedicatedServer={isDedicatedServer}, managerId={managerId}, "
                      + $"maxPlayerCount={configuration.maxPlayerCount}, "
                      + $"discoveryPolicy={configuration.discoveryPolicy}, "
                      + $"gameplayServerMode={configuration.gameplayServerMode}, "
                      + $"songSelectionMode={configuration.songSelectionMode})");

            PreConnectInfo = new PreConnectInfo(userId, userName, remoteEndPoint, secret, code, selectionMask,
                configuration, preMasterSecret, myRandom, remoteRandom, isConnectionOwner, isDedicatedServer,
                managerId);

            Current.Key = null;
            Current.ServerCode = code;
            Current.OwnerId = userId;
            Current.HostSecret = secret;
            Current.ManagerId = managerId; // BeatTogether incorrectly sends this as a decoded Platform User ID
            Current.PlayerLimit = configuration.maxPlayerCount;
            Current.GameplayMode = configuration.gameplayServerMode;
            Current.Name = null;
            Current.LobbyState = null;
            Current.MasterServerEndPoint = _serverBrowserClient.MasterServerEndPoint;
            Current.EndPoint = remoteEndPoint;
            Current.MultiplayerCoreVersion = _serverBrowserClient.MultiplayerCoreVersion;
            Current.MultiplayerExtensionsVersion = _serverBrowserClient.MultiplayerExtensionsVersion;
            
            Current.Level = null;
            Current.Players.Clear();

            if (selectionMask.difficulties != BeatmapDifficultyMask.All)
                Current.Difficulty = selectionMask.difficulties.FromMask();
            else
                Current.Difficulty = null;

            Current.ServerTypeCode = DetermineServerType();

            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(LobbyGameStateController), "StartMultiplayerLevel")]
        private void HandleStartMultiplayerLevel(IPreviewBeatmapLevel previewBeatmapLevel,
            BeatmapDifficulty beatmapDifficulty,
            BeatmapCharacteristicSO beatmapCharacteristic, IDifficultyBeatmap difficultyBeatmap,
            GameplayModifiers gameplayModifiers)
        {
            _log.Info($"Starting multiplayer level (levelID={previewBeatmapLevel.levelID}, " +
                      $"songName={previewBeatmapLevel.songName}, songSubName={previewBeatmapLevel.songSubName}, " +
                      $"songAuthorName={previewBeatmapLevel.songAuthorName}, " +
                      $"levelAuthorName={previewBeatmapLevel.levelAuthorName}, " +
                      $"difficulty={beatmapDifficulty}, characteristic={beatmapCharacteristic}, " +
                      $"modifiers={gameplayModifiers})");

            Current.Level = BssbServerLevel.FromDifficultyBeatmap(difficultyBeatmap);
            Current.Level.Characteristic = beatmapCharacteristic.serializedName;

            Current.Difficulty = difficultyBeatmap.difficulty;
            
            // TODO Capture modifiers? Maybe? Who cares?

            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(LobbyPlayersDataModel), "SetPlayerIsPartyOwner")]
        private void HandleSetPlayerIsPartyOwner(string userId, bool isPartyOwner)
        {
            if (!isPartyOwner)
                return;

            Current.ManagerId = userId;

            foreach (var player in Current.Players)
            {
                var wasPartyLeader = player.IsPartyLeader;
                player.IsPartyLeader = (player.UserId == userId);

                if (!wasPartyLeader && player.IsPartyLeader)
                {
                    var isLocalPlayer = _multiplayerSession.localPlayer.userId == player.UserId;

                    _log.Info($"Party leader changed to (userId={player.UserId}, " +
                              $"userName={player.UserName}, isMe={isLocalPlayer})");

                    if (isLocalPlayer && String.IsNullOrEmpty(Current.Name))
                        // Set server name if it wasn't set at session start (workaround for BeatTogether hosts)
                        Current.Name = _serverBrowserClient.PreferredServerName;
                }
            }

            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}