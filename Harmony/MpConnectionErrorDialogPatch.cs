using HarmonyLib;
using ServerBrowser.Game;
using ServerBrowser.Utils;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "PresentConnectionErrorDialog", MethodType.Normal)]
    public class MpConnectionErrorDialogPatch
    {
        public static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, ConnectionFailedReason reason)
        {
            Plugin.Log.Warn($"Multiplayer connection failed, reason: {reason}");
            if (MpModeSelection.WeInitiatedConnection && reason != ConnectionFailedReason.ConnectionCanceled)
            {
                // We only take over error handling UI if we initiated the connection, and it's not a self-cancel
                MpModeSelection.PresentConnectionFailedError
                (
                    errorMessage: ConnectionErrorText.Generate(reason),
                    canRetry: reason != ConnectionFailedReason.InvalidPassword && reason != ConnectionFailedReason.VersionMismatch
                );
                return false;
            }
            return true;
        }
    }
}