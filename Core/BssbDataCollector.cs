using System;
using System.Linq;
using System.Net;
using MultiplayerCore.Patchers;
using ServerBrowser.Models;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbDataCollector : IInitializable, IDisposable, IAffinity
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly IMultiplayerSessionManager _multiplayerSession = null!;
        [Inject] private readonly NetworkConfigPatcher _mpCoreNetConfig = null!;

        public bool SessionActive { get; private set; }
        public BssbServerDetail Current { get; private set; } = null!;
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

            Reset();
        }

        public void Dispose()
        {
            _multiplayerSession.connectedEvent -= HandleSessionConnected;
            _multiplayerSession.disconnectedEvent -= HandleSessionDisconnected;
            _multiplayerSession.playerConnectedEvent -= HandlePlayerConnected;
            _multiplayerSession.playerDisconnectedEvent -= HandlePlayerDisconnected;
        }

        private void Reset()
        {
            SessionActive = false;
            Current = new BssbServerDetail();

            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandleSessionConnected()
        {
            _log.Info($"Multiplayer session connected (syncTime={_multiplayerSession.syncTime})");

            SessionActive = true;

            if (IsPartyLeader)
            {
                // If we're the instance creator, session "maxPlayerCount" will be what they entered on Create Server
                //  Otherwise, this value won't be updated so we can't use it
                
                // This value also does NOT work for BeatTogether as their master doesn't set the right Manager ID, but
                //  that's not an issue because their maxPlayerCount is accurate in the pre connect info.
                
                Current.PlayerLimit = _multiplayerSession.maxPlayerCount;
                _log.Info($"MaxPlayerCount updated to {Current.PlayerLimit} (workaround for official servers)");
            }

            HandlePlayerConnected(_multiplayerSession.connectionOwner);
            HandlePlayerConnected(_multiplayerSession.localPlayer);

            if (Current.IsBeatTogetherHost)
            {
                _log.Info("Detected a BeatTogether host");
            }
            
            DataChanged?.Invoke(this, EventArgs.Empty);
            SessionEstablished?.Invoke(this, Current);
        }

        private void HandleSessionDisconnected(DisconnectedReason reason)
        {
            _log.Info($"Multiplayer session disconnected (reason={reason})");

            Reset();

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

            if (playerToRemove != null)
                Current.Players.Remove(playerToRemove);

            DataChanged?.Invoke(this, EventArgs.Empty);
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
            Current.OwnerId = userId;
            Current.ServerCode = code;
            Current.HostSecret = secret;
            Current.PlayerLimit = configuration.maxPlayerCount; // Official servers always seem to report 5
            Current.ManagerId = managerId; // BeatTogether incorrectly sends this as a decoded Platform User ID
            Current.EndPoint = remoteEndPoint;
            Current.GameplayMode = configuration.gameplayServerMode;

            if (_mpCoreNetConfig.MasterServerEndPoint is not null)
                Current.MasterServerEndPoint = _mpCoreNetConfig.MasterServerEndPoint;

            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(LobbyGameStateController), "StartMultiplayerLevel")]
        private void HandleStartLevel(IPreviewBeatmapLevel previewBeatmapLevel, BeatmapDifficulty beatmapDifficulty,
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

            // TODO Capture modifiers? Maybe? Who cares?

            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(LobbyPlayersDataModel), "SetPlayerIsPartyOwner")]
        private void HandleSetPartyOwner(string userId, bool isPartyOwner)
        {
            if (!isPartyOwner)
                return;

            Current.ManagerId = userId;

            foreach (var player in Current.Players)
            {
                player.IsPartyLeader = (player.UserId == userId);

                if (player.IsPartyLeader)
                {
                    var isLocalPlayer = _multiplayerSession.localPlayer.userId == player.UserId;
                    _log.Info($"Party leader changed to (userId={player.UserId}, " +
                              $"userName={player.UserName}, isMe={isLocalPlayer})");
                }
            }
        }
    }
}