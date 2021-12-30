using BeatSaberMarkupLanguage;
using ServerBrowser.Game;
using ServerBrowser.UI.ViewControllers;
using UnityEngine;
using static HMUI.ViewController;
using UnityEngine;

namespace ServerBrowser.UI
{
    public static class PluginUi
    {
        private static ServerBrowserViewController? _serverBrowserViewController;
        public static ServerBrowserViewController ServerBrowserViewController
        {
            get
            {
                if (_serverBrowserViewController == null)
                {
                    _serverBrowserViewController = BeatSaberUI.CreateViewController<ServerBrowserViewController>();
                }

                return _serverBrowserViewController;
            }
        }

        public static void SetUp()
        {
            FloatingNotification.SetUp();
        }

        public static void TearDown()
        {
            FloatingNotification.TearDown();

            if (_serverBrowserViewController is not null)
            {
                Object.Destroy(_serverBrowserViewController.gameObject);
                _serverBrowserViewController = null;
            }
        }

        public static void LaunchServerBrowser()
        {
            MpModeSelection.PresentViewController(ServerBrowserViewController, animationDirection: AnimationDirection.Horizontal);
        }
    }
}