using HarmonyLib;
using ServerBrowser.Game;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "DidDeactivate", MethodType.Normal)]
    public static class MpModeSelectionDeactivatedPatch
    {
        public static void Postfix(MultiplayerModeSelectionFlowCoordinator __instance, bool removedFromHierarchy, bool screenSystemDisabling)
        {
            // Fire closed event, but only if there is no current connection
            if (MpLobbyConnectionTypePatch.ConnectionType ==
                MultiplayerLobbyConnectionController.LobbyConnectionType.None)
            {
                MpEvents.RaiseOnlineMenuClosed(__instance);
            }
        }
    }
}
