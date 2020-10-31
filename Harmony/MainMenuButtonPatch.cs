using HarmonyLib;
using ServerBrowser.Core;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch updates the UI so it doesn't look broken outside of multiplayer.
    /// This won't be needed anymore if we figure out how to hide our modifier panel in non-multiplayer menus.
    /// </summary>
    [HarmonyPatch(typeof(MainMenuViewController), "HandleMenuButton", MethodType.Normal)]
    public class MainMenuButtonPatch
    {
        static void Prefix(MainMenuViewController.MenuButton menuButton)
        {
            // A main menu button was pressed
            GameStateManager.HandleUpdate();
        }
    }
}
