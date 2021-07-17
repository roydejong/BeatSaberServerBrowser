using HarmonyLib;
using ServerBrowser.Game;
using ServerBrowser.Utils;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "PresentConnectionErrorDialog", MethodType.Normal)]
    public class MpConnectionErrorDialogPatch
    {
        public static bool Prefix(ConnectionFailedReason reason)
        {
            Plugin.Log.Warn($"Multiplayer connection failed, reason: {reason}");
            
            if (MpModeSelection.WeInitiatedConnection)
            {
                if (MpModeSelection.WeAbortedJoin)
                {
                    MpModeSelection.PresentConnectionFailedError(
                        errorMessage: "The selected server instance is no longer available."
                    );
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
                        canRetry: reason != ConnectionFailedReason.InvalidPassword
                                  && reason != ConnectionFailedReason.VersionMismatch
                    );
                }
                
                return false;
            }
            
            return true;
        }
    }
}