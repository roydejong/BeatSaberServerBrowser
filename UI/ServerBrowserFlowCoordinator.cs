using HMUI;
using Zenject;

namespace ServerBrowser.UI
{
    public class ServerBrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private ServerBrowserMainViewController _mainViewController = null!;
        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Server Browser");
            showBackButton = true;
            
            ProvideInitialViewControllers(_mainViewController);
        }
    }
}