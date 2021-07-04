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
            if (MpModeSelection.WeInitiatedConnection)
            {
                // We only take over error handling UI if we initiated the connection
                if (MpModeSelection.InjectQuickPlaySecret != null)
                {
                    // ...if it's a a Quick Play join, show specific error
                    MpModeSelection.PresentConnectionFailedError("Quick Play join failed", 
                        "Sorry, it doesn't look like this Quick Play lobby is available right now.", true);
                    return false;
                }
                if (reason == ConnectionFailedReason.ConnectionCanceled)
                {
                    // ...and if it's just a self-cancel, return to the browser immediately.
                    MpModeSelection.CancelLobbyJoin();
                    MpModeSelection.MakeServerBrowserTopView();
                }
                else
                {
                    MpModeSelection.PresentConnectionFailedError
                    (
                        errorMessage: ConnectionErrorText.Generate(reason),
                        canRetry: reason != ConnectionFailedReason.InvalidPassword && reason != ConnectionFailedReason.VersionMismatch
                    );
                }
                return false;
            }
            return true;
        }
    }
}