using HarmonyLib;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.UI;
using static MultiplayerModeSelectionViewController;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "HandleMultiplayerLobbyControllerDidFinish", MethodType.Normal)]
    public static class MpModeSelectionButtonPatch
    {
        public static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, MultiplayerModeSelectionViewController viewController, MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            // Make sure any overrides are cleared when we're going to connect or host
            MpConnect.ClearMasterServerOverride();

            if (menuButton == MenuButton.GameBrowser)
            {
                // When the "GameBrowser" button is clicked, bypass the game's own incomplete code & open our view instead
                PluginUi.LaunchServerBrowser();
                return false;
            }
            else
            {
                // Going to a non-serverbrowser part of the online menu
                GlobalModState.Reset();
            }

            return true;
        }
    }
}
