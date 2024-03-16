using System.Threading.Tasks;
using BeatSaber.AvatarCore;
using HMUI;
using ServerBrowser.Data;
using ServerBrowser.Session;
using ServerBrowser.UI.Browser.Views;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.UI.Browser
{
    public class BrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly ServerRepository _serverRepository = null!;
        [Inject] private readonly BssbSession _session = null!;

        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly MainBrowserViewController _mainViewController = null!;

        [Inject] private readonly DiContainer _diContainer = null!;
        [Inject] private readonly GameServerLobbyFlowCoordinator _gameServerLobbyFlowCoordinator = null!;
        [Inject] private readonly IUnifiedNetworkPlayerModel _unifiedNetworkPlayerModel = null!;
        [Inject] private readonly LobbyDataModelsManager _lobbyDataModelsManager = null!;
        [Inject] private readonly FadeInOutController _fadeInOutController = null!;
        [Inject] private readonly MultiplayerLobbyConnectionController _multiplayerLobbyConnectionController = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;
        [Inject] private readonly ILobbyGameStateController _lobbyGameStateController = null!;
        [Inject] private readonly SimpleDialogPromptViewController _simpleDialogPromptViewController = null!;
        [Inject] private readonly AvatarSystemCollection _avatarSystemCollection = null!;
        [Inject] private readonly IMultiplayerSessionManager _multiplayerSessionManager = null!;

        private MultiplayerAvatarsData? _multiplayerAvatarsData = null;

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _mainViewController.ModeSelectedEvent += HandleModeSelected;
            _unifiedNetworkPlayerModel.connectedPlayerManagerCreatedEvent += HandleCpmCreated;

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

            _ = LoadAvatar();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _mainViewController.ModeSelectedEvent -= HandleModeSelected;
            _unifiedNetworkPlayerModel.connectedPlayerManagerCreatedEvent -= HandleCpmCreated;

            _serverRepository.StopDiscovery();

            _session.StopLoginRetries();
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

        private void HandleModeSelected(MainBrowserViewController.ModeSelectionTarget target)
        {
            switch (target)
            {
                case MainBrowserViewController.ModeSelectionTarget.EditAvatar:
                {
                    _mainFlowCoordinator._goToMultiplayerAfterAvatarCreation = true;
                    _mainFlowCoordinator._editAvatarFlowCoordinatorHelper.Show(_mainFlowCoordinator.childFlowCoordinator, true);
                    break;
                }
            }
        }

        private async Task LoadAvatar()
        {
            _multiplayerAvatarsData = await _avatarSystemCollection.GetMultiplayerAvatarsData(_playerDataModel
                .playerData
                .selectedAvatarSystemTypeId);

            _unifiedNetworkPlayerModel.connectedPlayerManager?.SetLocalPlayerAvatar(_multiplayerAvatarsData.Value);
        }

        private void HandleCpmCreated(INetworkPlayerModel networkPlayerModel)
        {
            _log.Info("Connected player manager created");

            if (_multiplayerAvatarsData != null)
                networkPlayerModel.connectedPlayerManager.SetLocalPlayerAvatar(_multiplayerAvatarsData.Value);
        }
    }
}