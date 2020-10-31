using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using ServerBrowser.UI;
using ServerBrowser.Utils;

namespace ServerBrowser.Harmony
{

    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "HandleMultiplayerLobbyControllerDidFinish", MethodType.Normal)]
    class MpModeSelectionButtonPatch
    {
        static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, MultiplayerModeSelectionViewController viewController, MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            // When the "GameBrowser" button is clicked, bypass the game's own incomplete code & open our view instead

            if (menuButton == MultiplayerModeSelectionViewController.MenuButton.GameBrowser)
            {
                GameMp.PresentServerBrowserView();
                return false;
            }

            return true;
        }
    }
}
