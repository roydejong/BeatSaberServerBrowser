using HarmonyLib;
using LobbyBrowserMod.Core;
using LobbyBrowserMod.UI;

namespace LobbyBrowserMod.Harmony
{
    class LobbyPlayerConnectedPatch
    {
        /// <summary>
        /// This patch helps us send an update to the master server if the player count changes.
        /// </summary>
        [HarmonyPatch(typeof(MultiplayerSessionManager), "HandlePlayerConnected", MethodType.Normal)]
        static void Postfix(IConnectedPlayer player, MultiplayerSessionManager __instance)
        {
            Plugin.Log?.Info($"Player connected: {player.userId}, {player.userName}");

            LobbyStateManager.HandleUpdate();
        }
    }
}
