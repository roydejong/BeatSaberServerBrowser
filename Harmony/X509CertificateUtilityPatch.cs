using HarmonyLib;
using ServerBrowser.Game;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(X509CertificateUtility), "ValidateCertificateChain")]
    public static class X509CertificateUtilityPatch
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