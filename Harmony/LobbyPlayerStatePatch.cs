using HarmonyLib;
using LobbyBrowserMod.Core;

namespace LobbyBrowserMod.Harmony
{
    [HarmonyPatch(typeof(MultiplayerSessionManager), "HandlePlayerStateChanged", MethodType.Normal)]
    class LobbyPlayerStatePatch
    {
        static void Postfix(IConnectedPlayer player)
        {
            // We are only interested in state changes from the connection owner at this time
            if (player.isConnectionOwner)
            {
                Plugin.Log?.Debug($"Host player state changed: {player.userId}, {player.userName}");
                LobbyStateManager.HandleUpdate();
            }
            
        }
    }
}
