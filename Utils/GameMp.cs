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

            mpConnectionController.ConnectToParty(serverCode);

            //this._joiningLobbyViewController.Init(Localization.Get("LABEL_JOINING_GAME"));
            //base.ReplaceTopViewController(this._joiningLobbyViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical);
        }
    }
}
