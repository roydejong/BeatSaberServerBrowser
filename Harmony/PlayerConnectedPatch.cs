using HarmonyLib;
using ServerBrowser;
using ServerBrowser.Core;

namespace LobbyBrowserMod.Harmony
{
    class PlayerConnectedPatch
    {
        /// <summary>
        /// This patch helps us send an update to the master server if the player count changes.
        /// </summary>
        [HarmonyPatch(typeof(MultiplayerSessionManager), "HandlePlayerConnected", MethodType.Normal)]
        static void Postfix(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"Player connected: {player.userId}, {player.userName}");
            LobbyStateManager.HandleUpdate();
        }
    }
}
