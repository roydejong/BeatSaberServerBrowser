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
            get => Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().First();
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
                LobbyBrowserViewController.Instance, null, ViewController.AnimationDirection.Horizontal, false
            });

            mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("SetTitle", new object[] {
                "Lobby Browser", ViewController.AnimationType.In
            });
        }

        public static void ShowModeSelectionError(string title, string message)
        {
            //var mpFlowCoordinator = ModeSelectionFlowCoordinator;
            ////var mpSimpleDialogPromptViewController = ReflectionUtil.GetField<SimpleDialogPromptViewController, MultiplayerModeSelectionFlowCoordinator>(mpFlowCoordinator, "_simpleDialogPromptViewController");

            //mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("PresentConnectionErrorDialog", new object[] {
            //    ConnectionFailedReason.MasterServerNotAuthenticated
            //});

            //mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("DismissViewController", new object[] {
            //    LobbyBrowserViewController.Instance, ViewController.AnimationDirection.Horizontal, null, false
            //});

            //mpSimpleDialogPromptViewController.Init(title, message, "OK", delegate (int btnId)
            //{
            //    mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("DismissViewController", new object[] {
            //        mpSimpleDialogPromptViewController, ViewController.AnimationDirection.Vertical, null, false
            //    });
            //});

            //mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("ReplaceTopViewController", new object[] {
            //    mpSimpleDialogPromptViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical
            //});
        }
    }
}
