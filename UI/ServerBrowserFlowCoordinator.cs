using System;
using HMUI;
using ServerBrowser.Models;
using ServerBrowser.UI.Utils;
using ServerBrowser.UI.Views;
using Zenject;

namespace ServerBrowser.UI
{
    public class ServerBrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly ServerBrowserMainViewController _mainViewController = null!;
        [Inject] private readonly ServerBrowserDetailViewController _detailViewController = null!;

        [Inject] private readonly MultiplayerModeSelectionFlowCoordinator _multiplayerModeSelectionFlowCoordinator =
            null!;

        [Inject] private readonly ModeSelectionIntegrator _modeSelectionIntegrator = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _mainViewController.RefreshStartedEvent += HandleRefreshStartedEvent;
            _mainViewController.CreateServerClickedEvent += HandleCreateServerClicked;
            _mainViewController.ServerSelectedEvent += HandleServerSelected;
            _mainViewController.ConnectClickedEvent += HandleConnectClicked;
            _detailViewController.ConnectClickedEvent += HandleConnectClicked;

            if (firstActivation)
            {
                SetTitle("Server Browser");
                showBackButton = true;

                ProvideInitialViewControllers(
                    mainViewController: _mainViewController,
                    rightScreenViewController: _detailViewController
                );
            }
            else
            {
                _detailViewController.ClearData();
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _mainViewController.RefreshStartedEvent -= HandleRefreshStartedEvent;
            _mainViewController.CreateServerClickedEvent -= HandleCreateServerClicked;
            _mainViewController.ServerSelectedEvent -= HandleServerSelected;
            _mainViewController.ConnectClickedEvent -= HandleConnectClicked;
            _detailViewController.ConnectClickedEvent -= HandleConnectClicked;
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            ReturnToModeSelection();
        }

        private async void HandleRefreshStartedEvent(object sender, EventArgs e)
        {
            await _detailViewController.Refresh();
        }

        private void HandleCreateServerClicked(object sender, EventArgs e)
        {
            ReturnToModeSelection(targetButton: MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }

        private async void HandleServerSelected(object sender, BssbServer? server)
        {
            if (server?.Key != null)
                await _detailViewController.LoadDetailsAsync(server.Key);
        }

        private void HandleConnectClicked(object sender, BssbServer server)
        {
            ReturnToModeSelection(targetServer: server);
        }

        private void ReturnToModeSelection(BssbServer? targetServer = null,
            MultiplayerModeSelectionViewController.MenuButton? targetButton = null)
        {
            var finishedCallback = new Action(() =>
            {
                if (targetServer is not null)
                    _modeSelectionIntegrator.ConnectToServer(targetServer);
                else if (targetButton is not null)
                    _modeSelectionIntegrator.TriggerMenuButton(targetButton.Value);
            });

            _mainFlowCoordinator.ReplaceChildFlowCoordinator(_multiplayerModeSelectionFlowCoordinator,
                finishedCallback, ViewController.AnimationDirection.Vertical, false);
        }
    }
}