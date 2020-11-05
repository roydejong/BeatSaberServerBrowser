using HarmonyLib;
using ServerBrowser.UI;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "HandleMultiplayerLobbyControllerDidFinish", MethodType.Normal)]
    public static class MpModeSelectionButtonPatch
    {
        public static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, MultiplayerModeSelectionViewController viewController, MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            // When the "GameBrowser" button is clicked, bypass the game's own incomplete code & open our view instead
            if (menuButton == MultiplayerModeSelectionViewController.MenuButton.GameBrowser)
            {
                PluginUi.LaunchServerBrowser();
                return false;
            }

            return true;
        }
    }
}
