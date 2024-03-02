using HMUI;
using Zenject;

namespace ServerBrowser.UI.Browser
{
    public class BrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly SimpleDialogPromptViewController _simpleDialogPromptViewController = null!;
        
        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Server Browser");
                showBackButton = true;
            }
                
            _simpleDialogPromptViewController.Init("Hello!", "Hi, how are you?", "Good", "Bad", (button) =>
            {
                ReturnToMainMenu();
            });

            if (firstActivation)
            {
                ProvideInitialViewControllers(_simpleDialogPromptViewController);
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