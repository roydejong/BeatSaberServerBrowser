using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using ServerBrowser.UI;
using ServerBrowser.UI.ViewControllers;
using System.Linq;
using UnityEngine;

namespace ServerBrowser.Utils
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

        public static bool LocalPlayerIsModded
        {
            get
            {
                var mpSessionManager = SessionManager;
                return mpSessionManager != null && mpSessionManager.LocalPlayerHasState("modded");
            }
        }

        public static void ConnectToServerCode(string serverCode)
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

        public static void OpenCreateServerMenu()
        {
            // If we are initiating the server menu from our UI, let's assume the intent is to host a game
            Plugin.Config.LobbyAnnounceToggle = true;

            var mpFlowCoordinator = ModeSelectionFlowCoordinator;
            mpFlowCoordinator.HandleMultiplayerLobbyControllerDidFinish(null, MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }
    }
}
