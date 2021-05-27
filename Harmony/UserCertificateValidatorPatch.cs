using HarmonyLib;
using ServerBrowser.Game;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(UserCertificateValidator), "ValidateCertificateChainInternal")]
    public static class UserCertificateValidatorPatch
    {
        public static bool Prefix()
        {
            // This mod disables certificate validation when it is overriding the master server to an unofficial one only.
            // If we are connecting to official games, we won't interfere.

            if (MpConnect.ShouldDisableCertificateValidation)
            {
                Plugin.Log?.Info("Bypassing user certificate validation");
                return false;
            }

            return true;
        }
    }
}