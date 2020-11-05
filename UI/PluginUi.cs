using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using ServerBrowser.Assets;
using ServerBrowser.UI.ViewControllers;
using ServerBrowser.Utils;
using System.Linq;
using UnityEngine;

namespace ServerBrowser.UI
{
    public static class PluginUi
    {
        private static ServerBrowserViewController _serverBrowserViewController;

        public static void SetUp()
        {
            FloatingNotification.SetUp();

            FloatingNotification.Instance.ShowMessageWithImageDownload(
                "Hello world!",
                "Do you like art?",
                "https://cdn.britannica.com/s:700x500/87/2087-004-264616BB/Mona-Lisa-oil-wood-panel-Leonardo-da.jpg",
                FloatingNotification.NotificationStyle.Red,
                Sprites.BeatSaverIcon,
                10.0f
            );
        }

        public static void LaunchServerBrowser() 
        {
            if (_serverBrowserViewController == null)
            {
                _serverBrowserViewController = BeatSaberUI.CreateViewController<ServerBrowserViewController>();
            }

            var mpFlowCoordinator = GameMp.ModeSelectionFlowCoordinator;

            mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("PresentViewController", new object[] {
                _serverBrowserViewController, null, ViewController.AnimationDirection.Vertical, false
            });
        }
    }
}
