using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSaber.AvatarCore;
using BGLib.Polyglot;
using HMUI;
using IgnoranceCore;
using ServerBrowser.Data;
using ServerBrowser.Session;
using ServerBrowser.UI.Browser.Views;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.UI.Browser
{
    public class BrowserFlowCoordinator : FlowCoordinator, IAffinity
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly ServerRepository _serverRepository = null!;
        [Inject] private readonly BssbSession _session = null!;

        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly MainBrowserViewController _mainViewController = null!;
        [Inject] private readonly JoiningLobbyViewController _joiningLobbyViewController = null!;
        [Inject] private readonly SimpleDialogPromptViewController _simpleDialogPromptViewController = null!;

        [Inject] private readonly DiContainer _diContainer = null!;
        [Inject] private readonly GameServerLobbyFlowCoordinator _gameServerLobbyFlowCoordinator = null!;
        [Inject] private readonly IUnifiedNetworkPlayerModel _unifiedNetworkPlayerModel = null!;
        [Inject] private readonly LobbyDataModelsManager _lobbyDataModelsManager = null!;
        [Inject] private readonly FadeInOutController _fadeInOutController = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;
        [Inject] private readonly ILobbyGameStateController _lobbyGameStateController = null!;
        [Inject] private readonly AvatarSystemCollection _avatarSystemCollection = null!;
        [Inject] private readonly IMultiplayerSessionManager _multiplayerSessionManager = null!;
        
        private CancellationTokenSource? _joiningLobbyCancellationTokenSource;
        private MultiplayerAvatarsData? _multiplayerAvatarsData;
        private ServerRepository.ServerInfo? _serverInfo;
        private ConnectionFailedReason? _connectionFailedReason;

        #region Setup / Flow coordinator
        
        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _mainViewController.ModeSelectedEvent += HandleModeSelected;
            _mainViewController.ServerJoinRequestedEvent += HandleServerJoinRequested;
            _joiningLobbyViewController.didCancelEvent += HandleJoinCanceled;
            
            _multiplayerSessionManager.connectedEvent += HandleSessionConnected;
            _multiplayerSessionManager.connectionFailedEvent += HandleSessionConnectionFailed;
            _unifiedNetworkPlayerModel.connectedPlayerManagerCreatedEvent += HandleCpmCreated;
            _unifiedNetworkPlayerModel.connectedPlayerManagerDestroyedEvent += HandleCpmDestroyed;

            if (firstActivation)
            {
                showBackButton = true;
                ProvideInitialViewControllers(_mainViewController);
                SetTitle("Online");
            }

            _serverRepository.StartDiscovery();

            _ = LoadAvatar();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _mainViewController.ModeSelectedEvent -= HandleModeSelected;
            _mainViewController.ServerJoinRequestedEvent -= HandleServerJoinRequested;
            _joiningLobbyViewController.didCancelEvent -= HandleJoinCanceled;
            
            _multiplayerSessionManager.connectedEvent -= HandleSessionConnected;
            _multiplayerSessionManager.connectionFailedEvent -= HandleSessionConnectionFailed;
            _unifiedNetworkPlayerModel.connectedPlayerManagerCreatedEvent -= HandleCpmCreated;

            _serverRepository.StopDiscovery();

            _session.StopLoginRetries();
        }

        // ReSharper disable once ParameterHidesMember
        public override void BackButtonWasPressed(ViewController topViewController)
        {
            if (_multiplayerSessionManager.isConnectingOrConnected)
                // Failsafe: Back button should not be visible right now
                return;
            
            ReturnToMainMenu();
        }

        public void ReturnToMainMenu()
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
        
        #endregion

        #region UI Events
        
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

        private void HandleServerJoinRequested(ServerRepository.ServerInfo server)
        {
            ConnectToServer(server);
        }
        
        private void HandleJoinCanceled()
        {
            _log.Info("User canceled server join");
            _connectionFailedReason = ConnectionFailedReason.ConnectionCanceled;
            DisconnectFromServer();
        }
        
        #endregion

        #region Connect / Disconnect API

        public void CreateServer(CreateServerFormData formData)
        {
            if (_multiplayerSessionManager.isConnectingOrConnected)
            {
                _log.Warn("Already connecting or connected, ignoring create server request");
                return;
            }
            
            throw new NotImplementedException("TODO");
            
            // MLCC:
            //  1. Create _partyConfig with a generated secret
            //   secret = NetworkUtility.GenerateId()
            //  2. CreatePartyConnection(), check success
            //  3. _multiplayerSessionManager.SetMaxPlayerCount
        }
        
        public void ConnectToServer(ServerRepository.ServerInfo serverInfo)
        {
            if (_multiplayerSessionManager.isConnectingOrConnected)
            {
                _log.Warn("Already connecting or connected, ignoring join request");
                return;
            }
            
            _log.Info($"Connect to server: {serverInfo.ServerName}, {serverInfo.ServerEndPoint}, " +
                      $"{serverInfo.ConnectionMethod}");

            _serverInfo = serverInfo;
            _connectionFailedReason = null;
            
            // We are doing the "connect by server code" flow, providing secret/code as available
            // We'll set default selection mask / configuration but the master server will override it
            // In case of direct connect, our GameLift patch will handle the rest
            var partyConfig = new UnifiedNetworkPlayerModel.JoinMatchmakingPartyConfig()
            {
                selectionMask = _serverInfo.BeatmapLevelSelectionMask ?? DefaultLevelSelectionMask,
                configuration = _serverInfo.GameplayServerConfiguration ?? DefaultGameplayServerConfiguration,
                secret = serverInfo.ServerSecret,
                code = serverInfo.ServerCode,
            };
            
            _joiningLobbyCancellationTokenSource = new();
            
            if (!_unifiedNetworkPlayerModel.CreatePartyConnection(partyConfig))
            {
                // CFR-1: Failed to create party connection
                HandleSessionConnectionFailed(ConnectionFailedReason.Unknown);
                return;
            }
            
            ShowJoiningLobby(serverInfo);
        }

        public void DisconnectFromServer(bool showError = true)
        {
            _joiningLobbyCancellationTokenSource?.Cancel();
            
            _unifiedNetworkPlayerModel.DestroyPartyConnection();
            
            if (showError)
                ShowConnectionError();
        }
        
        #endregion

        #region UI Show Views
        
        private void ShowMainView()
        {
            if (topViewController == _mainViewController)
                return;
            
            showBackButton = true;
            ReplaceTopViewController(_mainViewController, 
                animationType: ViewController.AnimationType.Out,
                animationDirection: ViewController.AnimationDirection.Vertical);
            SetTitle("Online");
        }

        private void ShowJoiningLobby(ServerRepository.ServerInfo serverInfo)
        {
            _joiningLobbyViewController.Init($"Joining {serverInfo.ServerName}...");
            
            showBackButton = false;
            ReplaceTopViewController(_joiningLobbyViewController,
                animationDirection: ViewController.AnimationDirection.Vertical);
            SetTitle("Online");
        }

        private void ShowConnectionError()
        {
            if (topViewController == _simpleDialogPromptViewController)
                return;
            
            if (_connectionFailedReason == ConnectionFailedReason.ConnectionCanceled)
            {
                // User canceled: return to main view
                ShowMainView();
                return;
            }

            _connectionFailedReason ??= ConnectionFailedReason.Unknown;
            
            var errTitle = Localization.Get("LABEL_CONNECTION_ERROR");
            var errKey = _connectionFailedReason.Value.LocalizedKey();
            var errCode = _connectionFailedReason.Value.ErrorCode();
            var errMsg = $"{Localization.Get(errKey)} ({errCode})";
            var btnOk = Localization.Get("BUTTON_OK");
            var btnRetry = Localization.Get("BUTTON_RETRY");

            _simpleDialogPromptViewController.Init(errTitle, errMsg, btnOk, btnRetry,
                (int btnId) =>
                {
                    if (btnId == 1)
                    {
                        // Retry
                        ConnectToServer(_serverInfo!);
                    }
                    else
                    {
                        // OK
                        ShowMainView();
                    }
                });
            
            showBackButton = false;
            ReplaceTopViewController(_simpleDialogPromptViewController, 
                animationType: ViewController.AnimationType.In,
                animationDirection: ViewController.AnimationDirection.Vertical);
            SetTitle("");
        }

        #endregion
        
        #region Multiplayer Connection - Base Game

        private async Task LoadAvatar()
        {
            _multiplayerAvatarsData = await _avatarSystemCollection.GetMultiplayerAvatarsData(_playerDataModel
                .playerData
                .selectedAvatarSystemTypeId);

            _unifiedNetworkPlayerModel.connectedPlayerManager?.SetLocalPlayerAvatar(_multiplayerAvatarsData.Value);
        }

        private void HandleCpmCreated(INetworkPlayerModel networkPlayerModel)
        {
            if (_multiplayerAvatarsData != null)
                networkPlayerModel.connectedPlayerManager.SetLocalPlayerAvatar(_multiplayerAvatarsData.Value);
            
            _multiplayerSessionManager.StartSession(MultiplayerSessionManager.SessionType.Player, 
                _unifiedNetworkPlayerModel.connectedPlayerManager);
        }

        private void HandleCpmDestroyed(INetworkPlayerModel obj)
        {
        }

        private void HandleSessionConnected()
        {
            _log.Info("Multiplayer session connected");
            
            _lobbyDataModelsManager.Activate();

            _ = StartLobbyFlowCoordinator();
        }

        private async Task StartLobbyFlowCoordinator()
        {
            await _lobbyGameStateController.GetGameStateAndConfigurationAsync(
                _joiningLobbyCancellationTokenSource!.Token);
            
            _log.Info("Lobby join successful");
            
            _serverRepository.StopDiscovery();

            _fadeInOutController.FadeOut(() =>
            {
                _gameServerLobbyFlowCoordinator.didFinishEvent -= HandleGameServerLobbyFlowCoordinatorDidFinish;
                _gameServerLobbyFlowCoordinator.didFinishEvent += HandleGameServerLobbyFlowCoordinatorDidFinish;
                _gameServerLobbyFlowCoordinator.willFinishEvent -= HandleGameServerLobbyFlowCoordinatorWillFinish;
                _gameServerLobbyFlowCoordinator.willFinishEvent += HandleGameServerLobbyFlowCoordinatorWillFinish;
                
                PresentFlowCoordinator(_gameServerLobbyFlowCoordinator, immediately: true,
                    replaceTopViewController: false);
                
                // Hack needed to setup UI properly because we're not replacing the top view controller
                // TODO: Can we do better?
                _gameServerLobbyFlowCoordinator.TopViewControllerWillChange(_joiningLobbyViewController,
                    _gameServerLobbyFlowCoordinator._lobbySetupViewController, 
                    ViewController.AnimationType.In);
            });
        }

        private void HandleGameServerLobbyFlowCoordinatorDidFinish()
        {
            _log.Info("Lobby has ended, returning to menu");
            
            _gameServerLobbyFlowCoordinator.didFinishEvent -= HandleGameServerLobbyFlowCoordinatorDidFinish;
            DismissFlowCoordinator(_gameServerLobbyFlowCoordinator, immediately: true);
            _joiningLobbyViewController.HideLoading();
            DisconnectFromServer();
            _lobbyDataModelsManager.Deactivate();
            _fadeInOutController.FadeIn();
        }

        private void HandleGameServerLobbyFlowCoordinatorWillFinish()
        {
            _gameServerLobbyFlowCoordinator.willFinishEvent -= HandleGameServerLobbyFlowCoordinatorWillFinish;
            _lobbyDataModelsManager.Deactivate();

            if (_connectionFailedReason == null)
                // Default: return to main browser view. Lobby flow coordinator will have shown its own errors.
                _connectionFailedReason = ConnectionFailedReason.ConnectionCanceled;
        }

        private void HandleSessionConnectionFailed(ConnectionFailedReason reason)
        {
            _log.Info($"Multiplayer session connection failed: {reason}");
            _connectionFailedReason = reason;
            DisconnectFromServer();
        }

        public static BeatmapLevelSelectionMask DefaultLevelSelectionMask =>
            new(BeatmapDifficultyMask.All, GameplayModifierMask.All, SongPackMask.all);

        public static GameplayServerConfiguration DefaultGameplayServerConfiguration =>
            new(5, DiscoveryPolicy.WithCode, InvitePolicy.AnyoneCanInvite, GameplayServerMode.Countdown, 
                SongSelectionMode.Vote, GameplayServerControlSettings.All);
        
        #endregion

        #region Multiplayer Connection - Patches

        [AffinityPrefix]
        [AffinityPatch(typeof(IgnoranceClient), "Start")]
        private void PrefixIgnoranceClientStart(IgnoranceClient __instance)
        {
            if (_serverInfo == null)
                // We are not managing this connection
                return;

            var enableDtls = _serverInfo.ConnectionMethod is ServerRepository.ConnectionMethod.GameLiftOfficial or
                ServerRepository.ConnectionMethod.GameLiftEncrypted;
            
            __instance.UseSsl = enableDtls;
            __instance.ValidateCertificate = enableDtls;
        }
        
        [AffinityPrefix]
        [AffinityPatch(typeof(GameLiftConnectionManager), nameof(GameLiftConnectionManager.GameLiftConnectToServer))]
        private bool PrefixGameLiftConnectToServer(string secret, string code, GameLiftConnectionManager __instance)
        {
            if (_serverInfo is not { ConnectionMethod: ServerRepository.ConnectionMethod.DirectConnect })
                // Patch does not apply to this connection
                return true;

            // We will skip the entire GameLift API and move to immediate connection
            __instance.HandleConnectToServerSuccess
            (
                playerSessionId: "DirectConnect",
                hostName: _serverInfo.ServerEndPoint!.Address.ToString(),
                port: _serverInfo.ServerEndPoint.Port,
                gameSessionId: _serverInfo.ServerUserId,
                secret: secret,
                code: code,
                selectionMask: DefaultLevelSelectionMask,
                configuration: DefaultGameplayServerConfiguration
            );
            return false;
        }

        #endregion
    }
}