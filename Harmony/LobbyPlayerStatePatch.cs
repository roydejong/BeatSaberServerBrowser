using HarmonyLib;
using LobbyBrowserMod.Core;

namespace LobbyBrowserMod.Harmony
{
    [HarmonyPatch(typeof(MultiplayerSessionManager), "HandlePlayerStateChanged", MethodType.Normal)]
    class LobbyPlayerStatePatch
    {
        static void Postfix(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"Player state change: {player.userId}, {player.userName} (isConnectionOwner: {player.isConnectionOwner})");

            LobbyStateManager.HandleUpdate();
        }
    }
}
