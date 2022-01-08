using BeatSaberMarkupLanguage;
using HMUI;
using ServerBrowser.UI.Utils;
using Zenject;

namespace ServerBrowser.UI
{
    public class ServerBrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly ServerBrowserMainViewController _mainViewController = null!;
        [Inject] private readonly MultiplayerModeSelectionFlowCoordinator _multiplayerModeSelectionFlowCoordinator = null!;

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
            BeatSaberUI.MainFlowCoordinator.ReplaceChildFlowCoordinator(_multiplayerModeSelectionFlowCoordinator,
               null, ViewController.AnimationDirection.Vertical, false);
        }
    }
}