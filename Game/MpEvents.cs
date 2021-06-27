using System;
using ServerBrowser.Game.Models;
using static MultiplayerLobbyConnectionController;

namespace ServerBrowser.Game
{
    /// <summary>
    /// Providers global multiplayer events for the server browser.
    /// </summary>
    internal static class MpEvents
    {
        #region ActivityUpdated
        /// <summary>
        /// This event is raised when the local multiplayer state changes.
        /// This is the "main" event to subscribe to if you don't need granular updates.
        /// </summary>
        internal static event EventHandler<MultiplayerActivity>? ActivityUpdated;

        internal static void RaiseActivityUpdated(object sender, MultiplayerActivity activity) =>
            ActivityUpdated.RaiseEventSafe(sender, activity);
        #endregion
        
        #region LobbyStateChanged
        /// <summary>
        /// This event is raised when the lobby connection type is set.
        /// </summary>
        internal static event EventHandler<MultiplayerLobbyState>? LobbyStateChanged;

        internal static void RaiseLobbyStateChanged(object sender, MultiplayerLobbyState lobbyState) =>
            LobbyStateChanged.RaiseEventSafe(sender, lobbyState);
        #endregion

        #region ConnectionTypeChanged
        /// <summary>
        /// This event is raised when the lobby state is set.
        /// </summary>
        internal static event EventHandler<LobbyConnectionType>? ConnectionTypeChanged;

        internal static void RaiseConnectionTypeChanged(object sender, LobbyConnectionType connectionType) => 
            ConnectionTypeChanged.RaiseEventSafe(sender, connectionType);
        #endregion

        #region BeforeConnectToServer
        /// <summary>
        /// This event is raised when the client is about to attempt to connect to a server.
        /// This is raised when joining a quick play or client game, but not when hosting.
        /// </summary>
        internal static event EventHandler<ConnectToServerEventArgs>? BeforeConnectToServer;

        internal static void RaiseBeforeConnectToServer(object sender, ConnectToServerEventArgs e) =>
            BeforeConnectToServer.RaiseEventSafe(sender, e);
        #endregion
        
        #region MasterServerChanged
        /// <summary>
        /// This event is raised when the master server endpoint is set.
        /// </summary>
        internal static event EventHandler<MasterServerEndPoint>? MasterServerChanged;

        internal static void RaiseMasterServerChanged(object sender, MasterServerEndPoint endPoint) =>
            MasterServerChanged.RaiseEventSafe(sender, endPoint);
        #endregion
        
        #region ServerCodeChanged
        /// <summary>
        /// This event is raised when the server code is set for a lobby.
        /// </summary>
        internal static event EventHandler<string>? ServerCodeChanged;

        internal static void RaiseServerCodeChanged(object sender, string code) =>
            ServerCodeChanged.RaiseEventSafe(sender, code);
        #endregion

        #region StartingMultiplayerLevel
        /// <summary>
        /// This event is raised when a multiplayer level is about to start.
        /// </summary>
        internal static event EventHandler<StartingMultiplayerLevelEventArgs>? StartingMultiplayerLevel;

        internal static void RaiseStartingMultiplayerLevel(object sender, StartingMultiplayerLevelEventArgs e) =>
            StartingMultiplayerLevel.RaiseEventSafe(sender, e);
        #endregion

        #region PlayerConnected
        /// <summary>
        /// This event is raised when a player connects to the multiplayer session.
        /// </summary>
        internal static event EventHandler<IConnectedPlayer>? PlayerConnected;

        internal static void RaisePlayerConnected(object sender, IConnectedPlayer player) =>
            PlayerConnected.RaiseEventSafe(sender, player);
        #endregion

        #region PlayerDisconnected
        /// <summary>
        /// This event is raised when a player connects to the multiplayer session.
        /// </summary>
        internal static event EventHandler<IConnectedPlayer>? PlayerDisconnected;

        internal static void RaisePlayerDisconnected(object sender, IConnectedPlayer player) =>
            PlayerDisconnected.RaiseEventSafe(sender, player);
        #endregion

        #region SessionStarted
        /// <summary>
        /// This event is raised when the multiplayer session has connected.
        /// This is also raised when a host has successfully created a new lobby.
        /// </summary>
        internal static event EventHandler<SessionConnectedEventArgs>? SessionConnected;

        internal static void RaiseSessionConnected(object sender, SessionConnectedEventArgs e) =>
            SessionConnected.RaiseEventSafe(sender, e);
        #endregion

        #region SessionDisconnected
        /// <summary>
        /// This event is raised when the multiplayer session has disconnected.
        /// </summary>
        internal static event EventHandler<DisconnectedReason>? SessionDisconnected;

        internal static void RaiseSessionDisconnected(object sender, DisconnectedReason reason) =>
            SessionDisconnected.RaiseEventSafe(sender, reason);
        #endregion
        
        #region Helper code
        private static void RaiseEventSafe<TArgs>(this EventHandler<TArgs>? e, object sender, TArgs args)
        {
            if (e == null)
                return;
            
            Plugin.Log?.Info($"[MpEvents] Invoking event: {e.Method.Name} ({args})");

            foreach (var invocation in e.GetInvocationList())
            {
                try
                {
                    ((EventHandler<TArgs>) invocation).Invoke(sender, args);
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"[MpEvents] Error in event handler ({e.Method.Name}): {ex}");
                }
            }
        }
        #endregion
    }
}