using HMUI;
using ServerBrowser.UI.Browser.Views;
using Zenject;

namespace ServerBrowser.UI.Browser
{
    public class BrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly MainBrowserViewController _mainViewController = null!;
        
        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Online");
                showBackButton = true;
            }

            if (firstActivation)
            {
                ProvideInitialViewControllers(_mainViewController);
            }
        }

        // ReSharper disable once ParameterHidesMember
        public override void BackButtonWasPressed(ViewController topViewController)
        {
            ReturnToMainMenu();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
        }

        public void ReturnToMainMenu()
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}