using LobbyBrowserMod.Harmony;
using LobbyBrowserMod.UI;
using LobbyBrowserMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LobbyBrowserMod.Core
{
    public static class LobbyStateManager
    {
        private static string _lobbyCode = null;
        private static bool _didAnnounce = false;
        private static string _lastCodeAnnounced = null;

        public static string StatusText { get; private set; } = "Unknown status";
        public static bool HasErrored { get; private set; } = true;

        public static bool DidLeakCurrentCode
        {
            get
            {
                return !String.IsNullOrEmpty(_lastCodeAnnounced) && _lastCodeAnnounced == _lobbyCode;
            }
        }

        public static void HandleLobbyCode(string lobbyCode)
        {
            if (_lobbyCode != lobbyCode)
            {
                Plugin.Log.Info($"Got lobby server code: \"{lobbyCode}\"");

                _lobbyCode = lobbyCode;
                HandleUpdate();
            }
        }

        public static void HandleUpdate()
        {
            if (!LobbyConnectionTypePatch.IsPartyMultiplayer || !LobbyConnectionTypePatch.IsPartyHost)
            {
                // We are not in a party lobby, or we are not the host
                // Make sure any previous host announcements by us are cancelled and bail
                StatusText = "You must be the host of a custom multiplayer game.";
                HasErrored = true;

                UnAnnounce();

                LobbyConfigPanel.instance.UpdateState();
                return;
            }

            var sessionManager = GameMp.SessionManager;
            var gameCode = _lobbyCode;

            if (sessionManager == null || String.IsNullOrEmpty(gameCode) || !sessionManager.isConnectionOwner
                || sessionManager.localPlayer == null || !sessionManager.isConnected || sessionManager.maxPlayerCount == 1)
            {
                // We do not (yet) have the Server Code, or we're at an in-between state where things aren't ready yet
                StatusText = "Can't send announcement (invalid lobby state).";
                HasErrored = true;

                UnAnnounce();

                LobbyConfigPanel.instance.UpdateState();
                return;
            }

            var playerCount = sessionManager.connectedPlayers.Count + 1; // + 1 for ourselves, the host
            var playerLimit = sessionManager.maxPlayerCount;
            var gameName = $"{sessionManager.localPlayer.userName}'s game";
            var haveCustomSongsEnabled = sessionManager.localPlayer.HasState("modded")
                && sessionManager.localPlayer.HasState("customsongs");

            StatusText = "Announcing your game to the world..." + "\r\n\r\n"
                + gameName + "\r\n"
                + $"{playerCount} / {playerLimit} players" + "\r\n" 
                + (haveCustomSongsEnabled ? "Modded songs enabled" : "Vanilla lobby (no custom songs)");
            HasErrored = false;

            LobbyConfigPanel.instance.UpdateState();

            // TODO: Pack announce info into a single object and pass down to DoAnnounce
            // TODO: Announce only if we actually have a useful update (hash announce info object?)

            DoAnnounce();
        }

        private static void DoAnnounce()
        {
            Plugin.Log.Info("Sending host announcement ...");

            if (MasterServerApi.SendAnnounce()) // TODO Actually implement this
            {
                _didAnnounce = true;
                _lastCodeAnnounced = _lobbyCode;
            }
        }

        /// <summary>
        /// Ensures that any host announcements made by us are removed:
        ///  - If a previous announcement was made, a DELETE request is sent to the master server, removing it.
        ///  - If no previous announcement was made, or it was already deleted, this is a no-op.
        /// </summary>
        public static void UnAnnounce()
        {
            if (_didAnnounce)
            {
                // TODO Actually implement this
                Plugin.Log.Info("Cancelling host announcement ...");
            }
        }
    }
}
