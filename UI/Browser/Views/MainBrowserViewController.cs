using HMUI;
using ServerBrowser.UI.Toolkit;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public partial class MainBrowserViewController : ViewController, IInitializable
    {
        [Inject] private LayoutBuilder _layoutBuilder = null!;

        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;

        public void Initialize()
        {
            BuildLayout(_layoutBuilder.Init(this));
        }

        private void HandleEditAvatarClick()
        {
            _mainFlowCoordinator.DismissFlowCoordinator(_mainFlowCoordinator.childFlowCoordinator,
                finishedCallback: () =>
                {
                    _mainFlowCoordinator._goToMultiplayerAfterAvatarCreation = true;
                    _mainFlowCoordinator._editAvatarFlowCoordinatorHelper.Show(_mainFlowCoordinator, true);
                });
        }
    }
}