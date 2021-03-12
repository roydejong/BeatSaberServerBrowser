using HarmonyLib;
using MasterServer;
using ServerBrowser.Game;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(UserMessageHandler), "ValidateCertificateChain")]
    public static class UserMessageHandlerCertificatePatch
    {
        public static bool Prefix()
        {
            // This mod disables certificate validation when it is overriding the master server to an unofficial one only.
            // If we are connecting to official games, we won't interfere.

            if (MpConnect.ShouldDisableCertificateValidation)
            {
                return false;
            }

            return true;
        }
    }
}