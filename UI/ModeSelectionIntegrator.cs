using System;
using HMUI;
using IPA.Utilities;
using MultiplayerCore.Patchers;
using ServerBrowser.Models;
using ServerBrowser.UI.Utils;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModeSelectionIntegrator : IInitializable, IDisposable, IAffinity
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly MultiplayerModeSelectionFlowCoordinator _flowCoordinator = null!;
        [Inject] private readonly MultiplayerModeSelectionViewController _modeSelectionView = null!;
        [Inject] private readonly ServerBrowserFlowCoordinator _serverBrowserFlowCoordinator = null!;
        [Inject] private readonly NetworkConfigPatcher _mpCoreNetConfig = null!;

        private Button? _btnGameBrowser;
        private bool _statusCheckComplete;
        private MultiplayerModeSelectionViewController.MenuButton? _pendingMenuButtonTrigger;

        public void Initialize()
        {
            _btnGameBrowser = _modeSelectionView.GetField<Button, MultiplayerModeSelectionViewController>
                ("_gameBrowserButton");
            _statusCheckComplete = false;
            _pendingMenuButtonTrigger = null;
        }

        public void Dispose()
        {
            if (_btnGameBrowser != null)
            {
                _btnGameBrowser.gameObject.SetActive(false);
                _btnGameBrowser = null;
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "DidActivate")]
        private void HandleFlowCoordinatorActivation(bool firstActivation, bool addedToHierarchy,
            bool screenSystemEnabling)
        {
            _statusCheckComplete = false;

            if (_btnGameBrowser != null)
            {
                _btnGameBrowser.gameObject.SetActive(true);

                if (firstActivation)
                {
                    // Move up and enlarge the button a bit
                    var transform = _btnGameBrowser.gameObject.transform;
                    transform.position += new Vector3(0, .4f, 0);
                    transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                }
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "ProcessDeeplinkingToLobby")]
        private bool HandleProcessDeeplinkingToLobby()
        {
            // ProcessDeeplinkingToLobby is triggered once the flow coordinator has fully set up, and transitions
            //  are completed. This means status checks are done and we can trigger secondary actions now

            _statusCheckComplete = true;

            if (_pendingMenuButtonTrigger is not null)
            {
                TriggerMenuButton(_pendingMenuButtonTrigger.Value);
                return false;
            }

            return true;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerModeSelectionFlowCoordinator),
            "HandleMultiplayerLobbyControllerDidFinish")]
        private bool HandleMenuButtonPress(MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            if (menuButton == MultiplayerModeSelectionViewController.MenuButton.GameBrowser)
            {
                LaunchServerBrowser();
                return false;
            }

            return true;
        }

        private void LaunchServerBrowser()
        {
            if (!_flowCoordinator.isActivated)
                return;

            _mainFlowCoordinator.ReplaceChildFlowCoordinator(_serverBrowserFlowCoordinator, null,
                ViewController.AnimationDirection.Vertical);
        }

        public void TriggerMenuButton(MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            if (!_statusCheckComplete)
            {
                // We can't trigger the button immediately as it will break the server status check
                _pendingMenuButtonTrigger = menuButton;
                return;
            }

            _pendingMenuButtonTrigger = null;
            _modeSelectionView.InvokeMethod<object, MultiplayerModeSelectionViewController>(
                "HandleMenuButton", menuButton);
        }

        private void ReplaceTopViewController(ViewController viewController, Action? finishedCallback = null,
            ViewController.AnimationType animationType = ViewController.AnimationType.In,
            ViewController.AnimationDirection animationDirection = ViewController.AnimationDirection.Horizontal)
        {
            _flowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("ReplaceTopViewController",
                viewController, finishedCallback, animationType, animationDirection);
        }

        public void ConnectToServer(BssbServer server)
        {
            _log.Info($"Trying to connect to selected server (Key={server.Key}, Name={server.Name}, " +
                      $"GameplayMode={server.GameplayMode}, MasterServerEndPoint={server.MasterServerEndPoint}, " +
                      $"ServerCode={server.ServerCode}, HostSecret={server.HostSecret}, " +
                      $"ServerTypeCode={server.ServerTypeCode})");

            // Set master server
            if (server.IsGameLiftHost || server.IsOfficial || server.MasterServerEndPoint is null)
                _mpCoreNetConfig.UseOfficialServer();
            else 
                _mpCoreNetConfig.UseMasterServer(server.MasterServerEndPoint, null!);
            
            // Set up lobby destination via deeplink
            _flowCoordinator.Setup(new SelectMultiplayerLobbyDestination(server.HostSecret, server.ServerCode));

            // If we are already on mode selection, trigger deeplink now
            if (_statusCheckComplete)
                _flowCoordinator.ProcessDeeplinkingToLobby();
        }
    }
}