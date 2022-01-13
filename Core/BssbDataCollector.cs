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
    public class BssbDataCollector : IInitializable, IDisposable, IAffinity
    {
        [Inject] private SiraLog _log = null!;
        [Inject] private IMultiplayerSessionManager _multiplayerSession = null!;
        [Inject] private readonly NetworkConfigPatcher _mpCoreNetConfig = null!;

        public bool SessionActive { get; private set; }
        public BssbServerDetail Current { get; private set; } = null!;
        public PreConnectInfo? PreConnectInfo { get; private set; } = null;
        
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
        }

        private void HandleSessionConnected()
        {
            _log.Info($"Multiplayer session connected (syncTime={_multiplayerSession.syncTime})");

            SessionActive = true;

            HandlePlayerConnected(_multiplayerSession.connectionOwner);
            HandlePlayerConnected(_multiplayerSession.localPlayer);

            if (_multiplayerSession.localPlayer.userId == PreConnectInfo?.ManagerId)
                Current.PlayerLimit = _multiplayerSession.maxPlayerCount; // Only updated if we are instance creator
        }

        private void HandleSessionDisconnected(DisconnectedReason reason)
        {
            _log.Info($"Multiplayer session disconnected (reason={reason})");

            Reset();
        }

        private void HandlePlayerConnected(IConnectedPlayer player)
        {
            if (ContainsPlayer(player.userId))
                return;

            _log.Info($"Player connected to session (sortIndex={player.sortIndex}, userId={player.userId}, "
                      + $"userName={player.userName}, isMe={player.isMe}, isConnectionOwner={player.isConnectionOwner}, "
                      + $"currentLatency={player.currentLatency})");

            Current.Players.Add(BssbServerPlayer.FromConnectedPlayer(player));
        }

        private void HandlePlayerDisconnected(IConnectedPlayer player)
        {
            _log.Info($"Player disconnected from session (userId={player.userId}, userName={player.userName})");

            var playerToRemove = Current.Players.FirstOrDefault(p => p.UserId == player.userId);

            if (playerToRemove != null)
                Current.Players.Remove(playerToRemove);
        }

        public bool ContainsPlayer(string userId) => Current.Players.Any(p => p.UserId == userId);

        [AffinityPostfix]
        [AffinityPatch(typeof(MasterServerConnectionManager), "HandleConnectToServerSuccess")]
        public void HandlePreConnect(string userId, string userName, IPEndPoint remoteEndPoint, string secret,
            string code, BeatmapLevelSelectionMask selectionMask, GameplayServerConfiguration configuration,
            byte[] preMasterSecret, byte[] myRandom, byte[] remoteRandom, bool isConnectionOwner,
            bool isDedicatedServer, string managerId)
        {
            // nb: HandleConnectToServerSuccess just means "the master server gave us the info to connect"
            _log.Info($"Game will connect to server (userId={userId}, userName={userName}, " 
                + $"remoteEndPoint={remoteEndPoint}, secret={secret}, code={code}, "
                + $"isDedicatedServer={isDedicatedServer}, managerId={managerId}, "
                + $"maxPlayerCount={configuration.maxPlayerCount}, "
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
            Current.ManagerId = managerId;
            Current.EndPoint = remoteEndPoint;
            
            if (_mpCoreNetConfig.MasterServerEndPoint is not null)
                Current.MasterServerEndPoint = _mpCoreNetConfig.MasterServerEndPoint;
        }
    }
}