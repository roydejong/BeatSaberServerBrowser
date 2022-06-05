using System;
using ServerBrowser.Assets;
using ServerBrowser.UI.Components;
using Zenject;

namespace ServerBrowser.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbSessionNotifier : IInitializable
    {
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly IMultiplayerSessionManager _sessionManager = null!;
        [Inject] private readonly BssbFloatingAlert _floatingAlert = null!;

        private bool _isConnected = false;
        private DateTime? _connectedSince = null;

        private TimeSpan TimeSinceConnected =>
            _connectedSince.HasValue ? (DateTime.Now - _connectedSince.Value) : new TimeSpan(0);

        public void Initialize()
        {
            _sessionManager.connectedEvent += HandleSessionConnected;
            _sessionManager.disconnectedEvent += HandleSessionDisconnected;
            _sessionManager.playerConnectedEvent += HandlePlayerConnected;
            _sessionManager.playerDisconnectedEvent += HandlePlayerDisconnected;
        }

        private void HandleSessionConnected()
        {
            _isConnected = true;
            _connectedSince = DateTime.Now;
            
            _floatingAlert.DismissAllImmediate();
        }

        private void HandleSessionDisconnected(DisconnectedReason reason)
        {
            _isConnected = false;
            _connectedSince = null;
            
            _floatingAlert.DismissAllImmediate();
        }

        private void HandlePlayerConnected(IConnectedPlayer player)
        {
            if (!_config.EnableJoinNotifications)
                return;
            
            if (!_isConnected || TimeSinceConnected.TotalSeconds <= 3)
                // On join, "player connected" is raised for all players; don't notify unless we've been connected awhile
                return;

            _floatingAlert.PresentNotification(new BssbFloatingAlert.NotificationData
            (
                Sprites.PortalUser,
                $"{player.userName} joined!",
                $"{_sessionManager.connectedPlayerCount} players connected",
                BssbLevelBarClone.BackgroundStyle.SolidBlue
            ));
        }

        private void HandlePlayerDisconnected(IConnectedPlayer player)
        {
            if (!_config.EnableJoinNotifications)
                return;
            
            if (!_isConnected)
                // On disconnect, "player disconnected" is raised for all players; don't notify unless connected
                return;

            _floatingAlert.PresentNotification(new BssbFloatingAlert.NotificationData
            (
                Sprites.Portal,
                $"{player.userName} disconnected",
                _sessionManager.connectedPlayerCount > 1
                    ? $"{_sessionManager.connectedPlayerCount} players remaining"
                    : "You're all alone",
                BssbLevelBarClone.BackgroundStyle.SolidCerise
            ));
        }
    }
}