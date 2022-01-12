using System;
using HMUI;
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
        [Inject] private readonly MultiplayerModeSelectionFlowCoordinator _multiplayerModeSelectionFlowCoordinator = null!;
        [Inject] private readonly ModeSelectionIntegrator _modeSelectionIntegrator = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Server Browser");
                showBackButton = true;

                ProvideInitialViewControllers(
                    mainViewController: _mainViewController,
                    rightScreenViewController: _detailViewController
                );
            }

            _mainViewController.CreateServerClickedEvent += HandleCreateServerClicked;
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _mainViewController.CreateServerClickedEvent -= HandleCreateServerClicked;
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            ReturnToModeSelection(null);
        }

        private void HandleCreateServerClicked(object sender, EventArgs e)
        {
            ReturnToModeSelection(MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }

        private void ReturnToModeSelection(MultiplayerModeSelectionViewController.MenuButton? targetButton = null)
        {
            var finishedCallback = new Action(() =>
            {
                if (targetButton is null)
                    return;
                
                _modeSelectionIntegrator.TriggerMenuButton(targetButton.Value);
            });
            
            _mainFlowCoordinator.ReplaceChildFlowCoordinator(_multiplayerModeSelectionFlowCoordinator,
                finishedCallback, ViewController.AnimationDirection.Vertical, false);
        }
    }
}