using System.Threading;
using HarmonyLib;
using IPA.Utilities;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// Temporary fix: Ensures the _joiningLobbyCancellationTokenSource is initialized, which fixes base game deep links
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "ProcessDeeplinkingToLobby", MethodType.Normal)]
    public static class FixDeepLinkJoins
    {
        public static void Prefix(MultiplayerModeSelectionFlowCoordinator __instance)
        {
            Plugin.Log.Debug("Hotfix ProcessDeeplinkingToLobby");
            __instance.SetField("_joiningLobbyCancellationTokenSource", new CancellationTokenSource());
        }
    }
}