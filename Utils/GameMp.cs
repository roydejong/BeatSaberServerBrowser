using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using LobbyBrowserMod.UI;
using System.Linq;
using UnityEngine;

namespace LobbyBrowserMod.Utils
{
    public static class GameMp
    {
        public static MultiplayerSessionManager SessionManager
        {
            get => Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().FirstOrDefault();
        }

        public static MultiplayerModeSelectionFlowCoordinator ModeSelectionFlowCoordinator
        {
            get => Resources.FindObjectsOfTypeAll<MultiplayerModeSelectionFlowCoordinator>().First();
        }

        public static string LastJoinAttemptServerCode
        {
            get;
            private set;
        }

        public static void JoinLobbyWithCode(string serverCode)
        {
            var mpFlowCoordinator = ModeSelectionFlowCoordinator;
            var mpConnectionController = ReflectionUtil.GetField<MultiplayerLobbyConnectionController, MultiplayerModeSelectionFlowCoordinator>(mpFlowCoordinator, "_multiplayerLobbyConnectionController");
            var mpJoiningLobbyViewController = ReflectionUtil.GetField<JoiningLobbyViewController, MultiplayerModeSelectionFlowCoordinator>(mpFlowCoordinator, "_joiningLobbyViewController");

            mpConnectionController.ConnectToParty(serverCode);
            LastJoinAttemptServerCode = serverCode;

            mpJoiningLobbyViewController.Init($"ohhh its happening ({serverCode})");

            mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("ReplaceTopViewController", new object[] {
                mpJoiningLobbyViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical
            });
        }

        public static void ShowLobbyBrowser()
        {
            var mpFlowCoordinator = ModeSelectionFlowCoordinator;

            mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("PresentViewController", new object[] {
                LobbyBrowserViewController.Instance, null, ViewController.AnimationDirection.Vertical, false
            });

            FixLobbyBrowserTitle();
        }
        public static void FixLobbyBrowserTitle()
        {
            var mpFlowCoordinator = ModeSelectionFlowCoordinator;

            mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("SetTitle", new object[] {
                "Server Browser", ViewController.AnimationType.In
            });
        }

        public static void OpenCreateServerMenu()
        {
            var mpFlowCoordinator = ModeSelectionFlowCoordinator;

            mpFlowCoordinator.HandleMultiplayerLobbyControllerDidFinish(null, MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }
    }
}
