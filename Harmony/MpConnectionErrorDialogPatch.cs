using HarmonyLib;
using ServerBrowser.Game;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "PresentConnectionErrorDialog", MethodType.Normal)]
    public class MpConnectionErrorDialogPatch
    {
        public static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, ConnectionFailedReason reason)
        {
            Plugin.Log.Warn($"Multiplayer connection failed, reason: {reason}");

            if (MpModeSelection.WeInitiatedConnection)
            {
                // We only take over error handling UI if we initiated the connection
                MpModeSelection.PresentConnectionError(reason);
                return false;
            }

            return true;
        }
    }
}
