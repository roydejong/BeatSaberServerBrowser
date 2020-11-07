using HMUI;
using IPA.Utilities;
using System.Linq;
using UnityEngine;
using static HMUI.ViewController;

namespace ServerBrowser.Game
{
    public static class MpModeSelection
    {
        private static MultiplayerModeSelectionFlowCoordinator _flowCoordinator;

        public static void SetUp()
        {
            _flowCoordinator = Resources.FindObjectsOfTypeAll<MultiplayerModeSelectionFlowCoordinator>().First();
        }

        public static void PresentViewController(ViewController viewController, AnimationDirection animationDirection = AnimationDirection.Vertical)
        {
            _flowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("PresentViewController", new object[] {
                viewController, null, animationDirection, false
            });
        }

        public static void SetTitle(string title)
        {
            _flowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("SetTitle", new object[] {
                title, ViewController.AnimationType.In
            });
        }

        public static void OpenCreateServerMenu()
        {
            Plugin.Config.LobbyAnnounceToggle = true; // If we are initiating the server menu from our UI, let's assume the intent is to host a game
            _flowCoordinator.HandleMultiplayerLobbyControllerDidFinish(null, MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }

        public static void ConnectToServerCode(string serverCode)
        {
            var mpConnectionController = ReflectionUtil.GetField<MultiplayerLobbyConnectionController, MultiplayerModeSelectionFlowCoordinator>(_flowCoordinator, "_multiplayerLobbyConnectionController");
            var mpJoiningLobbyViewController = ReflectionUtil.GetField<JoiningLobbyViewController, MultiplayerModeSelectionFlowCoordinator>(_flowCoordinator, "_joiningLobbyViewController");

            mpConnectionController.ConnectToParty(serverCode);

            mpJoiningLobbyViewController.Init($"ohhh its happening ({serverCode})");

            _flowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("ReplaceTopViewController", new object[] {
                mpJoiningLobbyViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical
            });
        }
    }
}
