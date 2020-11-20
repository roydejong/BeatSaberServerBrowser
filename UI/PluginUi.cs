using BeatSaberMarkupLanguage;
using ServerBrowser.Game;
using ServerBrowser.UI.ViewControllers;

namespace ServerBrowser.UI
{
    public static class PluginUi
    {
        private static ServerBrowserViewController _serverBrowserViewController;

        public static void SetUp()
        {
            FloatingNotification.SetUp();
        }

        public static void LaunchServerBrowser()
        {
            if (_serverBrowserViewController == null)
            {
                _serverBrowserViewController = BeatSaberUI.CreateViewController<ServerBrowserViewController>();
            }

            MpModeSelection.PresentViewController(_serverBrowserViewController);
        }
    }
}