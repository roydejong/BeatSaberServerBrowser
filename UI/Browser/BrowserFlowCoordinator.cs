using HMUI;
using ServerBrowser.Data;
using ServerBrowser.UI.Browser.Views;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.UI.Browser
{
    public class BrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly ServerRepository _serverRepository = null!;
        
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
            
            _serverRepository.StartDiscovery();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _serverRepository.StopDiscovery();
        }

        // ReSharper disable once ParameterHidesMember
        public override void BackButtonWasPressed(ViewController topViewController)
        {
            ReturnToMainMenu();
        }

        public void ReturnToMainMenu()
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}