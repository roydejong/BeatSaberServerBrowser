using HarmonyLib;
using LobbyBrowserMod.Core;
using ServerBrowser.Core;

namespace LobbyBrowserMod.Harmony
{
    /// <summary>
    /// This patch updates the UI so it doesn't look broken outside of multiplayer.
    /// This won't be needed anymore if we figure out how to hide our modifier panel in non-multiplayer menus.
    /// </summary>
    [HarmonyPatch(typeof(MainMenuViewController), "HandleMenuButton", MethodType.Normal)]
    public class MainMenuViewPatch
    {
        static void Prefix(MainMenuViewController.MenuButton menuButton)
        {
            // A main menu button was pressed
            LobbyStateManager.HandleUpdate();
        }
    }
}
