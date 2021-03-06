﻿using System;
using System.Linq;
using System.Threading;
using HMUI;
using IPA.Utilities;
using ServerBrowser.Core;
using ServerBrowser.UI;
using UnityEngine;
using static HMUI.ViewController;

namespace ServerBrowser.Game
{
    public static class MpModeSelection
    {
        public static bool WeInitiatedConnection { get; set; } = false;
        public static bool WeAbortedJoin { get; set; } = false;
        public static HostedGameData? LastConnectToHostedGame { get; private set; } = null;

        #region Init
        private static MultiplayerModeSelectionFlowCoordinator _flowCoordinator;
        private static MultiplayerLobbyConnectionController _mpLobbyConnectionController;
        private static JoiningLobbyViewController _joiningLobbyViewController;
        private static SimpleDialogPromptViewController _simpleDialogPromptViewController;

        public static void SetUp()
        {
            _flowCoordinator = Resources.FindObjectsOfTypeAll<MultiplayerModeSelectionFlowCoordinator>().First();
            _mpLobbyConnectionController = ReflectionUtil.GetField<MultiplayerLobbyConnectionController, MultiplayerModeSelectionFlowCoordinator>(_flowCoordinator, "_multiplayerLobbyConnectionController");
            _joiningLobbyViewController = ReflectionUtil.GetField<JoiningLobbyViewController, MultiplayerModeSelectionFlowCoordinator>(_flowCoordinator, "_joiningLobbyViewController");
            _simpleDialogPromptViewController = ReflectionUtil.GetField<SimpleDialogPromptViewController, MultiplayerModeSelectionFlowCoordinator>(_flowCoordinator, "_simpleDialogPromptViewController");
        }
        #endregion

        #region Private method helpers
        public static void PresentViewController(ViewController viewController, Action finishedCallback = null, AnimationDirection animationDirection = AnimationDirection.Vertical, bool immediately = false)
        {
            _flowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("PresentViewController", new object[] {
                viewController, finishedCallback, animationDirection, immediately
            });
        }

        public static void ReplaceTopViewController(ViewController viewController, Action finishedCallback = null, ViewController.AnimationType animationType = ViewController.AnimationType.In, ViewController.AnimationDirection animationDirection = ViewController.AnimationDirection.Horizontal)
        {
            _flowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("ReplaceTopViewController", new object[] {
                viewController, finishedCallback, animationType, animationDirection
            });
        }

        public static void DismissViewController(ViewController viewController, ViewController.AnimationDirection animationDirection = ViewController.AnimationDirection.Horizontal, Action finishedCallback = null, bool immediately = false)
        {
            _flowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("DismissViewController", new object[] {
                viewController, animationDirection, finishedCallback, immediately
            });
        }

        public static void SetTitle(string title)
        {
            _flowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("SetTitle", new object[] {
                title, ViewController.AnimationType.In
            });
        }
        #endregion

        public static void OpenCreateServerMenu()
        {
            // Make sure any overrides are cleared when we're going to host
            MpConnect.ClearMasterServerOverride();

            // If we are initiating the server menu from our UI, assume the intent is to host a game
            Plugin.Config.LobbyAnnounceToggle = true;
            Plugin.Config.ShareQuickPlayGames = true;

            _flowCoordinator.HandleMultiplayerLobbyControllerDidFinish(null, MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }

        public static void ConnectToHostedGame(HostedGameData? game)
        {
            if (game == null)
                return;

            MpModeSelection.WeInitiatedConnection = true;
            MpModeSelection.WeAbortedJoin = false;
            MpModeSelection.LastConnectToHostedGame = game;
            
            Plugin.Log.Info("--> Connecting to lobby destination now" +
                            $" (ServerCode={game.ServerCode}, HostSecret={game.HostSecret}," +
                            $" ServerType={game.ServerType}, ServerBrowserId={game.Id})");

            _flowCoordinator.SetField("_joiningLobbyCancellationTokenSource", new CancellationTokenSource());
            
            _mpLobbyConnectionController.CreateOrConnectToDestinationParty(
                new MpLobbyDestination(game.ServerCode, game.HostSecret)
            );
            
            _joiningLobbyViewController.Init($"{game.GameName} ({game.ServerCode})");
            
            ReplaceTopViewController(_joiningLobbyViewController, animationType: AnimationType.In,
                animationDirection: AnimationDirection.Vertical);
        }

        public static void PresentConnectionFailedError(string errorTitle = "Connection failed", string errorMessage = null, bool canRetry = true)
        {
            CancelLobbyJoin();

            if (LastConnectToHostedGame == null)
                canRetry = false; // we don't have game info to retry with

            _simpleDialogPromptViewController.Init(errorTitle, errorMessage, "Back to browser", canRetry ? "Retry connection" : null, delegate (int btnId)
            {
                switch (btnId)
                {
                    default:
                    case 0: // Back to browser
                        MakeServerBrowserTopView();
                        break;
                    case 1: // Retry connection
                        ConnectToHostedGame(LastConnectToHostedGame);
                        break;
                }
            });

            ReplaceTopViewController(_simpleDialogPromptViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical);
        }

        public static void CancelLobbyJoin(bool hideLoading = true)
        {
            _mpLobbyConnectionController.LeaveLobby();
            
            if (hideLoading)
                _joiningLobbyViewController.HideLoading();
        }

        public static void MakeServerBrowserTopView()
        {
            ReplaceTopViewController(PluginUi.ServerBrowserViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical);
        }
    }
}
