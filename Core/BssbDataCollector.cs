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
                _log.Info("Detected a BeatTogether host");
            else if (Current.IsGameLiftHost)
                _log.Info("Detected an Amazon GameLift host");

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
        private void HandleMasterServerPreConnect(string remoteUserId, string remoteUserName, IPEndPoint remoteEndPoint,
            string secret, string code, BeatmapLevelSelectionMask selectionMask,
            GameplayServerConfiguration configuration, byte[] preMasterSecret, byte[] myRandom, byte[] remoteRandom,
            bool isConnectionOwner, bool isDedicatedServer, string managerId)
        {
            // nb: HandleConnectToServerSuccess just means "the master server gave us the info to connect"
            //  we are not yet successfully connected to a dedicated server instance

            _log.Info($"Game will connect to server (remoteUserId={remoteUserId}, remoteUserName={remoteUserName}, "
                      + $"remoteEndPoint={remoteEndPoint}, secret={secret}, code={code}, "
                      + $"isDedicatedServer={isDedicatedServer}, managerId={managerId}, "
                      + $"maxPlayerCount={configuration.maxPlayerCount}, "
                      + $"discoveryPolicy={configuration.discoveryPolicy}, "
                      + $"gameplayServerMode={configuration.gameplayServerMode}, "
                      + $"songSelectionMode={configuration.songSelectionMode})");

            PreConnectInfo = new PreConnectInfo(remoteUserId, remoteUserName, remoteEndPoint, secret, code,
                selectionMask, configuration, preMasterSecret, myRandom, remoteRandom, isConnectionOwner,
                isDedicatedServer, managerId);

            Current.ServerCode = code;
            Current.RemoteUserId = remoteUserId;
            Current.RemoteUserName = remoteUserName;
            Current.HostSecret = secret;
            Current.ManagerId = managerId; // BeatTogether incorrectly sends this as a decoded Platform User ID
            Current.PlayerLimit = configuration.maxPlayerCount;
            Current.GameplayMode = configuration.gameplayServerMode;
            Current.MasterServerEndPoint = _serverBrowserClient.MasterServerEndPoint;
            Current.EndPoint = remoteEndPoint;
            Current.MultiplayerCoreVersion = _serverBrowserClient.MultiplayerCoreVersion;
            Current.MultiplayerExtensionsVersion = _serverBrowserClient.MultiplayerExtensionsVersion;

            if (selectionMask.difficulties != BeatmapDifficultyMask.All)
                Current.Difficulty = selectionMask.difficulties.FromMask();
            else
                Current.Difficulty = null;

            FinishPreConnectHandling();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameLiftConnectionManager), "HandleConnectToServerSuccess")]
        private void HandleGameLiftPreConnect(string playerSessionId, IPEndPoint remoteEndPoint, string gameSessionId,
            string secret, string code, BeatmapLevelSelectionMask selectionMask,
            GameplayServerConfiguration configuration)
        {
            // nb: HandleConnectToServerSuccess just means "the GameLift API gave us the info to connect"
            //   we are not yet successfully connected to the dedicated server instance
            
            _log.Info($"Game will connect to GameLift session (playerSessionId={playerSessionId}, "
                      + $"remoteEndPoint={remoteEndPoint}, gameSessionId={gameSessionId}, secret={secret}, code={code}, "
                      + $"maxPlayerCount={configuration.maxPlayerCount}, "
                      + $"discoveryPolicy={configuration.discoveryPolicy}, "
                      + $"gameplayServerMode={configuration.gameplayServerMode}, "
                      + $"songSelectionMode={configuration.songSelectionMode})");
            
            // https://docs.aws.amazon.com/gamelift/latest/apireference/API_PlayerSession.html
            
            // playerSessionId is "psess-{GUID}"
            //  A unique identifier for a player session.

            // gameSessionId is an AWS identifier (ARN) starting with "arn:aws:gamelift:" and equals the dedi's user id
            //  A unique identifier for the game session that the player session is connected to.

            PreConnectInfo = new PreConnectInfo(playerSessionId, gameSessionId, remoteEndPoint,
                secret, code, selectionMask, configuration, null, null, null,
                false, true, null);

            Current.ServerCode = code;
            Current.RemoteUserId = gameSessionId;
            Current.RemoteUserName = null;
            Current.HostSecret = secret;
            Current.ManagerId = null;
            Current.PlayerLimit = configuration.maxPlayerCount;
            Current.GameplayMode = configuration.gameplayServerMode;
            Current.MasterServerEndPoint = null;
            Current.EndPoint = remoteEndPoint;

            if (selectionMask.difficulties != BeatmapDifficultyMask.All)
                Current.Difficulty = selectionMask.difficulties.FromMask();
            else
                Current.Difficulty = null;
            
            FinishPreConnectHandling();
        }

        private void FinishPreConnectHandling()
        {
            Current.Key = null;
            Current.Name = null;
            Current.LobbyState = null;
            Current.MultiplayerCoreVersion = _serverBrowserClient.MultiplayerCoreVersion;
            Current.MultiplayerExtensionsVersion = _serverBrowserClient.MultiplayerExtensionsVersion;

            Current.Level = null;
            Current.Players.Clear();

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