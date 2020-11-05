using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
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
            FloatingNotification.Instance.ShowMessage("Hello world!", "Have a poggers day!");
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
