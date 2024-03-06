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
        
        private void HandleQuickPlayClick()
        {
            Plugin.Log.Error($"Quick Play");
        }

        private void HandleCreateServerClick()
        {
            Plugin.Log.Error($"Create Server");
        }
        
        private void HandleJoinByCodeClick()
        {
            Plugin.Log.Error($"Join by Code");
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
        
        private void HandleSearchInputChanged(InputFieldView.SelectionState state, string value)
        {
            Plugin.Log.Error($"Search updated: {state}, \"{value}\"");
        }
        
        private void HandleFilterButtonClicked()
        {
            Plugin.Log.Error($"Filters clicked");
            _filterButton!.SetTextValue("ooh ya clicked me good");
        }
        
        private void HandleFilterButtonCleared()
        {
            Plugin.Log.Error($"Filters cleared");
        }
    }
}