using HarmonyLib;
using ServerBrowser.Game;
using ServerBrowser.UI;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "HandleMultiplayerLobbyControllerDidFinish", MethodType.Normal)]
    public static class MpModeSelectionButtonPatch
    {
        public static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, MultiplayerModeSelectionViewController viewController, MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            // Make sure any overrides are cleared when we're going to connect or host
            MpConnect.ClearMasterServerOverride();

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
