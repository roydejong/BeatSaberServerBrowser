using System;
using System.Linq;
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

        public bool SessionActive { get; private set; }
        public BssbServerDetail Current { get; private set; } = null!;

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
            _log.Info($"Multiplayer session connected (syncTime={_multiplayerSession.syncTime}, "
                      + $"maxPlayerCount={_multiplayerSession.maxPlayerCount})");

            SessionActive = true;

            HandlePlayerConnected(_multiplayerSession.localPlayer);

            Current.PlayerLimit = _multiplayerSession.maxPlayerCount;
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
    }
}