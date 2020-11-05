using ServerBrowser.Core;
using ServerBrowser.Harmony;
using ServerBrowser.UI;
using System.Linq;
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
        public static void Start()
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

        public static void Stop()
        {
            SessionManager.connectedEvent -= OnSessionConnected;
            SessionManager.disconnectedEvent -= OnSessionDisconnected;
            SessionManager.playerConnectedEvent -= OnSessionPlayerConnected;
            SessionManager.playerDisconnectedEvent -= OnSessionPlayerDisconnected;
        }
        #endregion

        #region Session events
        private static void OnSessionConnected()
        {
            Plugin.Log?.Info("Multiplayer session is now connected.");

            IsConnected = true;
            DisconnectedReason = DisconnectedReason.Unknown;
        }

        private static void OnSessionDisconnected(DisconnectedReason reason)
        {
            Plugin.Log?.Info($"Multiplayer session is now disconnected ({reason}).");

            IsConnected = false;
            DisconnectedReason = reason;
        }

        private static void OnSessionPlayerConnected(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"A player joined the session: {player.userName}");

            // State update, player count may have changed
            GameStateManager.HandleUpdate();

            // Notification if enabled (and fully connected, because all players raise this on connect)
            var isFullyConnected = IsConnected && MpLobbyConnectionTypePatch.ConnectionType != LobbyConnectionType.None;
            if (isFullyConnected && Plugin.Config.JoinNotificationsEnabled)
            {
                FloatingNotification.Instance.ShowMessage(
                    $"{player.userName} joined!",
                    $"{GetPlayerCount()}/{GetPlayerLimit()} players connected"
                );
            }
        }

        private static void OnSessionPlayerDisconnected(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"A player left the session: {player.userName}");

            // State update, player count may have changed
            GameStateManager.HandleUpdate();

            // Notification if enabled (and connected, because all players raise this on disconnect)
            var isFullyConnected = IsConnected && MpLobbyConnectionTypePatch.ConnectionType != LobbyConnectionType.None;
            if (isFullyConnected && Plugin.Config.JoinNotificationsEnabled)
            {
                FloatingNotification.Instance.ShowMessage(
                    $"{player.userName} disconnected",
                    $"{GetPlayerCount()}/{GetPlayerLimit()} players connected"
                );
            }
        }
        #endregion

        #region Data helpers
        public static bool GetLocalPlayerHasMultiplayerExtensions()
        {
            return SessionManager.LocalPlayerHasState("modded");
        }

        public static int GetPlayerCount()
        {
            if (!IsConnected)
                return 0;
            
            // NB: +1 because the local player doesn't count as a "connected player"
            return SessionManager.connectedPlayerCount + 1;
        }

        public static int GetPlayerLimit()
        {
            switch (MpLobbyConnectionTypePatch.ConnectionType)
            {
                case LobbyConnectionType.PartyClient:
                case LobbyConnectionType.PartyHost:
                    // Custom lobby with its own player limit
                    return SessionManager.maxPlayerCount;

                case LobbyConnectionType.QuickPlay:
                case LobbyConnectionType.None:
                default:
                    // Not connected yet, or in QuickPlay, assume 5
                    // (QuickPlay doesn't seem to set this value)
                    return 5;
            }
        }
        #endregion
    }
}
