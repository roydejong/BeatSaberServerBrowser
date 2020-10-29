using HMUI;
using IPA.Utilities;
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

        public static void JoinLobbyWithCode(string serverCode)
        {
            var mpFlowCoordinator = Resources.FindObjectsOfTypeAll<MultiplayerModeSelectionFlowCoordinator>().First();
            var mpConnectionController = ReflectionUtil.GetField<MultiplayerLobbyConnectionController, MultiplayerModeSelectionFlowCoordinator>(mpFlowCoordinator, "_multiplayerLobbyConnectionController");
            var mpJoiningLobbyViewController = ReflectionUtil.GetField<JoiningLobbyViewController, MultiplayerModeSelectionFlowCoordinator>(mpFlowCoordinator, "_joiningLobbyViewController");

            mpConnectionController.ConnectToParty(serverCode);
            mpJoiningLobbyViewController.Init("ohhh its happening");
            mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("ReplaceTopViewController", new object[] {
                mpJoiningLobbyViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical
            });
        }
    }
}
