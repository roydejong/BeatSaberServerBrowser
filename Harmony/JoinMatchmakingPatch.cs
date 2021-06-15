using System.Net;
using HarmonyLib;
using IPA.Utilities;
using ServerBrowser.Core;
using ServerBrowser.Game;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us inject a secret when connecting to a Quick Play game, so we can control which game is joined.
    /// </summary>
    [HarmonyPatch(typeof(UnifiedNetworkPlayerModel), "JoinMatchmaking")]
    public static class JoinMatchmakingPatch
    {
        public static void Prefix(GameplayServerConfiguration configuration, DiscoveryPolicy discoveryPolicy,
            ref string secret, ref string code)
        {
            if (MpModeSelection.WeInitiatedConnection && MpModeSelection.InjectQuickPlaySecret != null)
            {
                secret = MpModeSelection.InjectQuickPlaySecret;
                code = MpModeSelection.InjectServerCode;
                
                Plugin.Log?.Debug($"Injecting Quick Play secret: {secret}");
            }
        }
    }
}