using System;
using HMUI;
using ServerBrowser.Models;
using ServerBrowser.UI.Components;
using ServerBrowser.UI.Utils;
using ServerBrowser.UI.Views;
using Zenject;

namespace ServerBrowser.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerBrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly ServerBrowserMainViewController _mainViewController = null!;
        [Inject] private readonly ServerBrowserDetailViewController _detailViewController = null!;
        [Inject] private readonly MultiplayerModeSelectionFlowCoordinator _modeSelectionFlowCoordinator = null!;
        [Inject] private readonly ModeSelectionIntegrator _modeSelectionIntegrator = null!;
        [Inject] private readonly CoverArtLoader _coverArtLoader = null!;
        [Inject] private readonly BssbFloatingAlert _floatingAlert = null!;

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
                SetRightScreenViewController(_detailViewController, ViewController.AnimationType.In);

                _coverArtLoader.UnloadCache();

                ProvideInitialViewControllers(
                    mainViewController: _mainViewController,
                    rightScreenViewController: _detailViewController
                );
            }
            else
            {
                _detailViewController.ClearData();
            }

            _floatingAlert.DismissAllImmediate();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _mainViewController.RefreshStartedEvent -= HandleRefreshStartedEvent;
            _mainViewController.CreateServerClickedEvent -= HandleCreateServerClicked;
            _mainViewController.ServerSelectedEvent -= HandleServerSelected;
            _mainViewController.ConnectClickedEvent -= HandleConnectClicked;
            _detailViewController.ConnectClickedEvent -= HandleConnectClicked;

            if (removedFromHierarchy)
                _coverArtLoader.UnloadCache();

            _floatingAlert.DismissAllPending();
            _floatingAlert.DismissPinned();
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            ReturnToModeSelection();
        }

        private void HandleRefreshStartedEvent(object sender, EventArgs e)
        {
            _ = _detailViewController.Refresh();
        }

        private void HandleCreateServerClicked(object sender, EventArgs e)
        {
            _config.AnnounceParty = true; // turn the announce switch on when creating server from our UI

            ReturnToModeSelection(targetButton: MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }

        private void HandleServerSelected(object sender, BssbServer? server)
        {
            if (server?.Key != null)
                _ = _detailViewController.LoadDetailsAsync(server.Key, server.IsLocallyDiscovered);
        }

        private void HandleConnectClicked(object sender, BssbServer server) =>
            ReturnToModeSelection(targetServer: server);

        private void ReturnToModeSelection(BssbServer? targetServer = null,
            MultiplayerModeSelectionViewController.MenuButton? targetButton = null)
        {
            if (targetServer is not null)
                _modeSelectionIntegrator.SetMasterServerOverride(targetServer);

            var finishedCallback = new Action(() =>
            {
                if (targetServer is not null)
                    _modeSelectionIntegrator.ConnectToServer(targetServer);
                else if (targetButton is not null)
                    _modeSelectionIntegrator.TriggerMenuButton(targetButton.Value);
            });

            _mainFlowCoordinator.ReplaceChildFlowCoordinator(_modeSelectionFlowCoordinator,
                finishedCallback, ViewController.AnimationDirection.Vertical, false);
        }
    }
}