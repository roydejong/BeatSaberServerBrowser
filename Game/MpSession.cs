using System;
using System.Linq;
using ServerBrowser.Assets;
using ServerBrowser.Game.Models;
using ServerBrowser.Harmony;
using ServerBrowser.UI;
using ServerBrowser.Utils;
using UnityEngine;
using static MultiplayerLobbyConnectionController;

namespace ServerBrowser.Game
{
    public static class MpSession
    {
        /// <summary>
        /// Instance of the game's MultiplayerSessionManager.
        /// </summary>
        public static MultiplayerSessionManager SessionManager { get; private set; }
        /// <summary>
        /// Indicates whether we are currently connected to a multiplayer session or not.
        /// </summary>
        public static bool IsConnected { get; private set; } = false;
        /// <summary>
        /// The most recent reason we were disconnected from a multiplayer session.
        /// </summary>
        public static DisconnectedReason DisconnectedReason { get; private set; } = DisconnectedReason.Unknown;

        #region Lifecycle
        public static void SetUp()
        {
            /** 
             * Note: The game creates one session manager in MainSystemInit.InstallBindings(), so
             *   we have only one instance that we can use continuously, it won't change.
             */
            SessionManager = Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().FirstOrDefault();

            if (SessionManager == null)
            {
                Plugin.Log?.Critical("Unable to get MultiplayerSessionManager! Things are broken now.");
                return;
            }

            SessionManager.connectedEvent += OnSessionConnected;
            SessionManager.disconnectedEvent += OnSessionDisconnected;
            SessionManager.playerConnectedEvent += OnSessionPlayerConnected;
            SessionManager.playerDisconnectedEvent += OnSessionPlayerDisconnected;
        }

        public static void TearDown()
        {
            if (SessionManager != null)
            {
                SessionManager.connectedEvent -= OnSessionConnected;
                SessionManager.disconnectedEvent -= OnSessionDisconnected;
                SessionManager.playerConnectedEvent -= OnSessionPlayerConnected;
                SessionManager.playerDisconnectedEvent -= OnSessionPlayerDisconnected;
            }
        }
        #endregion

        #region Session events
        private static void OnSessionConnected()
        {
            Plugin.Log?.Info("Multiplayer session is now connected.");

            IsConnected = true;
            DisconnectedReason = DisconnectedReason.Unknown;
            
            // Raise internal event
            MpEvents.RaiseSessionConnected(SessionManager, new SessionConnectedEventArgs()
            {
                ConnectionOwner = SessionManager.connectionOwner,
                LocalPlayer = SessionManager.localPlayer,
                MaxPlayers = SessionManager.maxPlayerCount
            });
        }

        private static void OnSessionDisconnected(DisconnectedReason reason)
        {
            Plugin.Log?.Info($"Multiplayer session is now disconnected ({reason}).");

            IsConnected = false;
            DisconnectedReason = reason;

            // Restore the user's preferred master server
            MpConnect.ClearMasterServerOverride();

            // Clear any notifications
            FloatingNotification.Instance.DismissMessage();
            
            // Raise internal event
            MpEvents.RaiseSessionDisconnected(SessionManager, reason);
        }

        private static void OnSessionPlayerConnected(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"A player joined the session: {player.userName}");

            // Notification if enabled (and fully connected, because all players raise this on connect)
            var isFullyConnected = IsConnected && MpLobbyConnectionTypePatch.ConnectionType != LobbyConnectionType.None;

            if (isFullyConnected && Plugin.Config.JoinNotificationsEnabled)
            {
                FloatingNotification.Instance.ShowMessage(
                    $"{player.userName} joined!",
                    $"{GetPlayerCount()} players connected",
                    FloatingNotification.NotificationStyle.Blue,
                    Sprites.PortalUser
                );
            }
            
            // Raise internal event
            MpEvents.RaisePlayerConnected(SessionManager, player);
        }

        private static void OnSessionPlayerDisconnected(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"A player left the session: {player.userName}");

            // Notification if enabled (and connected, because all players raise this on disconnect)
            var isFullyConnected = IsConnected && MpLobbyConnectionTypePatch.ConnectionType != LobbyConnectionType.None;

            if (isFullyConnected && Plugin.Config.JoinNotificationsEnabled)
            {
                var playerCount = GetPlayerCount();

                FloatingNotification.Instance.ShowMessage(
                    $"{player.userName} disconnected",
                    playerCount > 1 ? $"{playerCount} players remaining" : "You're all alone",
                    FloatingNotification.NotificationStyle.Red,
                    Sprites.Portal
                );
            }
            
            // Raise internal event
            MpEvents.RaisePlayerDisconnected(SessionManager, player);
        }
        #endregion

        #region Data helpers
        public static bool GetLocalPlayerHasMultiplayerExtensions()
        {
            return SessionManager.LocalPlayerHasState("modded") || ModCheck.MultiplayerExtensions.InstalledAndEnabled;
        }

        public static int GetPlayerCount()
        {
            if (!IsConnected)
                return 0;
            
            // NB: +1 because the local player doesn't count as a "connected player"
            return SessionManager.connectedPlayerCount + 1;
        }

        public static string GetHostGameName()
        {
            if (MpLobbyConnectionTypePatch.ConnectionType == LobbyConnectionType.QuickPlay)
            {
                return "Quick Play Lobby";
            }
            
            string finalGameName = "";

            if (MpLocalPlayer.UserInfo != null)
            {
                finalGameName = $"{MpLocalPlayer.UserInfo.userName}'s game";
            }

            if (!String.IsNullOrEmpty(Plugin.Config.CustomGameName))
            {
                finalGameName = Plugin.Config.CustomGameName;
            }

            return !String.IsNullOrEmpty(finalGameName) ? finalGameName : "A game";
        }
        #endregion
    }
}
