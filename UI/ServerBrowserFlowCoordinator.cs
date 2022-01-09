using System;
using BeatSaberMarkupLanguage;
using HMUI;
using ServerBrowser.UI.Utils;
using Zenject;

namespace ServerBrowser.UI
{
    public class ServerBrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly ServerBrowserMainViewController _mainViewController = null!;
        [Inject] private readonly MultiplayerModeSelectionFlowCoordinator _multiplayerModeSelectionFlowCoordinator = null!;
        [Inject] private readonly ModeSelectionIntegrator _modeSelectionIntegrator = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                showBackButton = true;
                SetTitle("Server Browser");
                ProvideInitialViewControllers(_mainViewController);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
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