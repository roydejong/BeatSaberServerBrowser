using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using LobbyBrowserMod.UI;
using LobbyBrowserMod.Utils;

namespace LobbyBrowserMod.Harmony
{

    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "HandleMultiplayerLobbyControllerDidFinish", MethodType.Normal)]
    class MultiplayerModeSelectionFlowPatch
    {
        static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, MultiplayerModeSelectionViewController viewController, MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            // When the "GameBrowser" button is clicked, bypass the game's own incomplete code & open our view instead

            if (menuButton == MultiplayerModeSelectionViewController.MenuButton.GameBrowser)
            {
                GameMp.ShowLobbyBrowser();
                return false;
            }

            return true;
        }
    }
}
