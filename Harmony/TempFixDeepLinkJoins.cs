using System.Threading;
using HarmonyLib;
using IPA.Utilities;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// Temporary fix:
    /// It ensures the _joiningLobbyCancellationTokenSource is initialized, which fixes base game deep links.
    /// Since we use the deep link code to connect lobbies, this is essential until fixed by the base game.
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