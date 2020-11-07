using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using ServerBrowser.Game;
using ServerBrowser.UI.ViewControllers;
using ServerBrowser.Utils;

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
            ViewController viewToMount;

            if (Plugin.Config.UseNativeBrowserPreview)
            {
                // Native browser preview (in development)
                NativeServerBrowser.SetUp();

                viewToMount = NativeServerBrowser.ViewController;
            }
            else
            {
                // Original custom BSML based browser
                if (_serverBrowserViewController == null)
                {
                    _serverBrowserViewController = BeatSaberUI.CreateViewController<ServerBrowserViewController>();
                }

                viewToMount = _serverBrowserViewController;
            }

            if (viewToMount != null)
            {
                MpModeSelection.PresentViewController(viewToMount);
            }
        }
    }
}
