using HarmonyLib;

namespace LobbyBrowserMod.Harmony
{

    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "PresentConnectionErrorDialog", MethodType.Normal)]
    class ConnectionErrorDialogPatch
    {
        static void Prefix(ConnectionFailedReason reason)
        {
            Plugin.Log?.Info($"Connection error, reason: {reason}");

            // TODO: Send report back to server that the game is dead, depending on the reason? 
        }
    }
}
