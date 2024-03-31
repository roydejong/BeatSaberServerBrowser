using System.Threading;
using System.Threading.Tasks;
using BeatSaber.AvatarCore;
using BGLib.Polyglot;
using HMUI;
using IgnoranceCore;
using ServerBrowser.Data;
using ServerBrowser.UI.Browser.Views;
using ServerBrowser.Util;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
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
        [Inject] private readonly CreateServerViewController _createServerViewController = null!;
        [Inject] private readonly JoinQuickPlayViewController _joinQuickPlayViewController = null!;
        [Inject] private readonly ServerCodeEntryViewController _serverCodeEntryViewController = null!;
        [Inject] private readonly GameServerLobbyFlowCoordinator _gameServerLobbyFlowCoordinator = null!;
        
        [Inject] private readonly IMultiplayerSessionManager _multiplayerSessionManager = null!;
        [Inject] private readonly IUnifiedNetworkPlayerModel _unifiedNetworkPlayerModel = null!;
        [Inject] private readonly ILobbyGameStateController _lobbyGameStateController = null!;
        
        [Inject] private readonly FadeInOutController _fadeInOutController = null!;
        [Inject] private readonly MenuLightsManager _menuLightsManager = null!;
        
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;
        [Inject] private readonly LobbyDataModelsManager _lobbyDataModelsManager = null!;
        [Inject] private readonly AvatarSystemCollection _avatarSystemCollection = null!;
        [Inject] private readonly SongPackMasksModel _songPackMasksModel = null!;
         
        private CancellationTokenSource? _joiningLobbyCancellationTokenSource;
        private MultiplayerAvatarsData? _multiplayerAvatarsData;
        private ServerRepository.ServerInfo? _serverInfo;
        
        private bool _wasEverConnected;
        private DisconnectedReason? _realDisconnectReason;
        private ConnectionFailedReason? _connectionFailedReason;

        #region Setup / Flow coordinator
        
        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (addedToHierarchy)
            {
                _mainViewController.ServerJoinRequestedEvent += HandleServerJoinRequested;
                _mainViewController.ModeSelectedEvent += HandleModeSelected;
                _mainViewController.AvatarEditRequestedEvent += HandleAvatarEditRequested;

                _joiningLobbyViewController.didCancelEvent += HandleJoinCanceled;

                _joinQuickPlayViewController.didFinishEvent += HandleQuickPlayViewFinished;
                _createServerViewController.didFinishEvent += HandleCreateServerViewFinished;
                _serverCodeEntryViewController.didFinishEvent += HandleServerCodeViewFinished;

                _multiplayerSessionManager.connectedEvent += HandleSessionConnected;
                _multiplayerSessionManager.connectionFailedEvent += HandleSessionConnectionFailed;
                _unifiedNetworkPlayerModel.connectedPlayerManagerCreatedEvent += HandleCpmCreated;
                _unifiedNetworkPlayerModel.connectedPlayerManagerDestroyedEvent += HandleCpmDestroyed;
            }

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
            if (removedFromHierarchy)
            {
                _mainViewController.ServerJoinRequestedEvent -= HandleServerJoinRequested;
                _mainViewController.ModeSelectedEvent -= HandleModeSelected;
                _mainViewController.AvatarEditRequestedEvent -= HandleAvatarEditRequested;

                _joiningLobbyViewController.didCancelEvent -= HandleJoinCanceled;
            
                _joinQuickPlayViewController.didFinishEvent -= HandleQuickPlayViewFinished;
                _createServerViewController.didFinishEvent -= HandleCreateServerViewFinished;
                _serverCodeEntryViewController.didFinishEvent -= HandleServerCodeViewFinished;

                _multiplayerSessionManager.connectedEvent -= HandleSessionConnected;
                _multiplayerSessionManager.connectionFailedEvent -= HandleSessionConnectionFailed;
                _unifiedNetworkPlayerModel.connectedPlayerManagerCreatedEvent -= HandleCpmCreated;
            }

            _serverRepository.StopDiscovery();

            _session.StopLoginRetries();
        }

        // ReSharper disable once ParameterHidesMember
        public override void BackButtonWasPressed(ViewController topViewController)
        {
            if (_multiplayerSessionManager.isConnectingOrConnected || topViewController == _joiningLobbyViewController)
            {
                // Failsafe: Back button should not be visible right now
                return;
            }

            if (topViewController == _joinQuickPlayViewController ||
                topViewController == _createServerViewController ||
                topViewController == _serverCodeEntryViewController)
            {
                // Sub view is active, dismiss to return to top view
                ShowMainView();
                return;
            }

            if (topViewController == _mainViewController)
            {
                // Main view is active, exit to main menu
                ReturnToMainMenu();
                return;
            }
        }

        public void ReturnToMainMenu()
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
        
        #endregion

        #region UI Events
        
        private void HandleAvatarEditRequested()
        {
            _mainFlowCoordinator._goToMultiplayerAfterAvatarCreation = true; // will trigger patch in MainMenuIntegrator
            _mainFlowCoordinator._editAvatarFlowCoordinatorHelper.Show(this, true);
        }

        private void HandleServerJoinRequested(ServerRepository.ServerInfo server)
        {
            ConnectToServer(server);
        }
        
        private void HandleServerJoinByCodeRequested(string serverCode)
        {
            ConnectToServer(new ServerRepository.ServerInfo()
            {
                ServerName = "Lobby",
                ConnectionMethod = ServerRepository.ConnectionMethod.GameLiftOfficial,
                ServerCode = serverCode
            });
        }
        
        private void HandleJoinCanceled()
        {
            _log.Info("User canceled server join");
            _connectionFailedReason = ConnectionFailedReason.ConnectionCanceled;
            DisconnectFromServer();
        }
        
        #endregion

        #region Connect / Disconnect API

        public void JoinQuickPlay(BeatmapDifficultyMask beatmapDifficultyMask, SongPackMask songPackMask,
            bool allowSongSelection = true)
        {
            // TODO Master server select

            var selectionMask = new BeatmapLevelSelectionMask(beatmapDifficultyMask, GameplayModifierMask.NoFail,
                songPackMask);

            var serverConfig = new GameplayServerConfiguration(
                5,
                DiscoveryPolicy.Public,
                InvitePolicy.NobodyCanInvite,
                allowSongSelection ? GameplayServerMode.Countdown : GameplayServerMode.QuickStartOneSong,
                allowSongSelection ? SongSelectionMode.Vote : SongSelectionMode.Random,
                GameplayServerControlSettings.None
            );
            
            // Note: QuickStartOneSong / allowSongSelection=false is not currently used in the base game
            
            var serverInfo = new ServerRepository.ServerInfo()
            {
                Key = "quickplay",
                ServerName = "Quick Play",
                GameModeName = "Quick Play",
                PlayerCount = 0,
                PlayerLimit = serverConfig.maxPlayerCount,
                ConnectionMethod = ServerRepository.ConnectionMethod.GameLiftOfficial, // TODO Custom form value
                BeatmapLevelSelectionMask = selectionMask,
                GameplayServerConfiguration = serverConfig,
            };
            
            ConnectToServer(serverInfo);
        }

        public void CreateServer(CreateServerFormData formData)
        {
            var randomSecret = NetworkUtility.GenerateId();
            
            // TODO Master server select

            var selectionMask = new BeatmapLevelSelectionMask(formData.difficulties, formData.modifiers,
                formData.songPacks);

            var serverConfig = new GameplayServerConfiguration(
                formData.maxPlayers,
                formData.netDiscoverable ? DiscoveryPolicy.Public : DiscoveryPolicy.WithCode,
                formData.allowInviteOthers ? InvitePolicy.AnyoneCanInvite : InvitePolicy.OnlyConnectionOwnerCanInvite,
                formData.gameplayServerMode,
                formData.songSelectionMode,
                formData.gameplayServerControlSettings
            );
            
            var serverInfo = new ServerRepository.ServerInfo()
            {
                Key = "create",
                ServerName = "New Lobby", // TODO Custom form value
                GameModeName = "Custom",  // TODO Custom form value
                PlayerCount = 0,
                PlayerLimit = formData.maxPlayers,
                ConnectionMethod = ServerRepository.ConnectionMethod.GameLiftOfficial, // TODO Custom form value
                ServerSecret = randomSecret,
                BeatmapLevelSelectionMask = selectionMask,
                GameplayServerConfiguration = serverConfig
            };
            
            ConnectToServer(serverInfo);
        }
        
        public void ConnectToServer(ServerRepository.ServerInfo serverInfo)
        {
            if (_multiplayerSessionManager.isConnectingOrConnected)
            {
                _log.Warn("Already connecting or connected, ignoring join request");
                return;
            }
            
            if (serverInfo.ConnectionMethod == ServerRepository.ConnectionMethod.DirectConnect)
            {
                _log.Info($"Connecting to dedicated server (serverName={serverInfo.ServerName}, " +
                          $"endPoint={serverInfo.ServerEndPoint}, connectionMethod={serverInfo.ConnectionMethod}");
            }
            else
            {
                _log.Info($"Connecting to lobby via master server (serverName={serverInfo.ServerName}, " +
                          $"serverCode={serverInfo.ServerCode}, serverSecret={serverInfo.ServerSecret}, " +
                          $"connectionMethod={serverInfo.ConnectionMethod}");
            }

            _serverInfo = serverInfo;

            _wasEverConnected = false;
            _realDisconnectReason = null;
            _connectionFailedReason = null;
            
            // We are doing the "connect by server code" flow, providing secret/code as available
            // We'll set selection mask and configuration, but a master server may override it
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
                // CFR-1: Failed to create party connection - should never happen
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
                _ = ShowConnectionError();
        }
        
        #endregion

        #region Mode Selection Sub-views
        
        private void HandleModeSelected(MultiplayerModeSelectionViewController.MenuButton mode)
        {
            if (topViewController != _mainViewController)
                // Can only transition from main view
                return;

            var multiplayerModeSettings = _playerDataModel.playerData.multiplayerModeSettings;
            
            ViewController? nextViewController = null;
            switch (mode)
            {
                case MultiplayerModeSelectionViewController.MenuButton.QuickPlay:
                {
                    _joinQuickPlayViewController.Setup(new QuickPlaySetupData(), multiplayerModeSettings);
                    nextViewController = _joinQuickPlayViewController;
                    break;
                }
                case MultiplayerModeSelectionViewController.MenuButton.CreateServer:
                {
                    _createServerViewController.Setup(multiplayerModeSettings);
                    nextViewController = _createServerViewController;
                    break;
                }
                case MultiplayerModeSelectionViewController.MenuButton.JoinWithCode:
                {
                    nextViewController = _serverCodeEntryViewController;
                    break;
                }
            }
            
            if (nextViewController == null)
                return;
            
            ReplaceTopViewController(nextViewController, animationDirection: ViewController.AnimationDirection.Vertical);
        }

        private void HandleQuickPlayViewFinished(bool success)
        {
            if (topViewController != _joinQuickPlayViewController)
                return;
            
            if (!success)
            {
                ShowMainView();
                return;
            }

            var modeSettings = _joinQuickPlayViewController.multiplayerModeSettings;
            
            var difficultyMask = modeSettings.quickPlayBeatmapDifficulty;
            var songPackMask = _songPackMasksModel.ToSongPackMask(modeSettings.quickPlaySongPackMaskSerializedName);
            var allowSongSelection = modeSettings.quickPlayEnableLevelSelection;
            
            var logMode = allowSongSelection ? "Countdown" : "QuickStartOneSong";
            _log.Info($"Quick play join requested: {difficultyMask}, {songPackMask}, {logMode}");
            
            JoinQuickPlay(difficultyMask, songPackMask, allowSongSelection);
        }

        private void HandleCreateServerViewFinished(bool success, CreateServerFormData formData)
        {
            if (topViewController != _createServerViewController)
                return;
            
            if (!success)
            {
                ShowMainView();
                return;
            }
            
            _log.Info($"Create server requested: {formData.maxPlayers} players");
            
            CreateServer(formData);
        }
        
        private void HandleServerCodeViewFinished(bool success, string code)
        {
            if (topViewController != _serverCodeEntryViewController)
                return;
            
            if (!success)
            {
                ShowMainView();
                return;
            }
            
            _log.Info($"Join by code requested: {code}");
            
            HandleServerJoinByCodeRequested(code);
        }
        
        #endregion
        
        #region Connection UI
        
        private void ShowMainView()
        {
            if (topViewController == _mainViewController)
                return;
            
            showBackButton = true;
            ReplaceTopViewController(_mainViewController, 
                animationType: ViewController.AnimationType.Out,
                animationDirection: ViewController.AnimationDirection.Vertical);
            SetTitle("Online");
            ResetMenuLights();
        }

        private void ShowJoiningLobby(ServerRepository.ServerInfo serverInfo)
        {
            _joiningLobbyViewController.Init($"Joining {serverInfo.ServerName}...");
            
            showBackButton = false;
            ReplaceTopViewController(_joiningLobbyViewController,
                animationDirection: ViewController.AnimationDirection.Vertical);
            SetTitle("Online");
            SetMenuLights(MenuLightsConnectPreset);
        }

        private async Task ShowConnectionError()
        {
            while (_joiningLobbyViewController.isInTransition)
            {
                // Prevent showing the error dialog while the join view is still transitioning
                await Task.Delay(10);
            }
            
            if (topViewController == _simpleDialogPromptViewController)
                return;

            if (_connectionFailedReason == ConnectionFailedReason.ConnectionCanceled
                || _realDisconnectReason is DisconnectedReason.UserInitiated or DisconnectedReason.ClientConnectionClosed)
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
            
            // If the connection failed due to a disconnect, we can provide more useful error messages than the game can  
            if (_wasEverConnected && _realDisconnectReason != null)
            {
                errKey = _realDisconnectReason.Value.LocalizedKey();
                errCode = _realDisconnectReason.Value.ErrorCode();
                
                // Won't use base game localized messages here because they're not helpful
                errMsg = "Connection failed (disconnected)";
                switch (_realDisconnectReason)
                {
                    case DisconnectedReason.Timeout:
                        errMsg += "<br>The connection timed out";
                        break;
                    case DisconnectedReason.ServerAtCapacity:
                        errMsg += "<br>The server is full";
                        break;
                    case DisconnectedReason.Kicked:
                    case DisconnectedReason.ServerConnectionClosed: // base game would say: "Server was shut down" ???
                        errMsg += "<br>You were kicked by the server";
                        break;
                    case DisconnectedReason.ServerTerminated:
                        errMsg += "<br>The server is shutting down";
                        break;
                    default:
                        errMsg += "<br>Check your internet connection and try again";
                        break;
                }
                errMsg += $" ({errCode})";
                
                _log.Error($"Multiplayer connection failed with disconnect error: {errCode} ({errKey}): \"{errMsg}\"");
            }
            else
            {
                _log.Error($"Multiplayer connection failed error: {errCode} ({errKey}): \"{errMsg}\"");
            }

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
            SetMenuLights(MenuLightsErrorPreset);
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
        [AffinityPatch(typeof(MultiplayerSessionManager), nameof(MultiplayerSessionManager.UpdateConnectionState))]
        private void PrefixUpdateConnectionState(UpdateConnectionStateReason updateReason,
            DisconnectedReason disconnectedReason, ConnectionFailedReason connectionFailedReason)
        {
            // Read internal session updates to get actually useful error messages
            
            _log.Debug($"Connection state changed: updateReason={updateReason}," +
                      $" disconnectedReason={disconnectedReason}, connectionFailedReason={connectionFailedReason}");
            
            if (updateReason == UpdateConnectionStateReason.Connected && !_wasEverConnected)
            {
                _wasEverConnected = true;
                _log.Info("Connected to server");
            }

            if (disconnectedReason is not DisconnectedReason.Unknown and not DisconnectedReason.UserInitiated
                && _realDisconnectReason != disconnectedReason)
            {
                _realDisconnectReason = disconnectedReason;
                _log.Info($"Disconnect reason: {disconnectedReason} ({connectionFailedReason})");
            }

            if (connectionFailedReason == ConnectionFailedReason.ServerIsTerminating)
            {
                // Base game message is completely useless ("server does not exist"), so push our disconnect reason instead
                _realDisconnectReason = DisconnectedReason.ServerTerminated;
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(IgnoranceClient), nameof(IgnoranceClient.Start))]
        // ReSharper disable once InconsistentNaming
        private void PrefixIgnoranceClientStart(IgnoranceClient __instance)
        {
            if (_serverInfo == null)
                // We are not managing this connection - patch does not apply
                return;

            __instance.UseSsl = _serverInfo.UseDtlsEncryption;
            __instance.ValidateCertificate = _serverInfo.UseDtlsEncryption;
        }
        
        [AffinityPrefix]
        [AffinityPatch(typeof(GameLiftConnectionManager), nameof(GameLiftConnectionManager.GameLiftConnectToServer))]
        // ReSharper disable once InconsistentNaming
        private bool PrefixGameLiftConnectToServer(string secret, string code, GameLiftConnectionManager __instance)
        {
            if (_serverInfo is not { ConnectionMethod: ServerRepository.ConnectionMethod.DirectConnect })
                // We are not managing this connection, or it is a regular GameLift connection - patch does not apply
                return true;

            // We will skip the entire GameLift API and move to immediate connection
            __instance.HandleConnectToServerSuccess
            (
                playerSessionId: "DirectConnect",
                hostName: _serverInfo.ServerEndPoint!.Address.ToString(),
                port: _serverInfo.ServerEndPoint.Port,
                gameSessionId: _serverInfo.ServerUserId,
                secret: secret,
                code: "DIRECT",
                selectionMask: _serverInfo.BeatmapLevelSelectionMask ?? DefaultLevelSelectionMask,
                configuration: _serverInfo.GameplayServerConfiguration ?? DefaultGameplayServerConfiguration
            );
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameServerLobbyFlowCoordinator),
            nameof(GameServerLobbyFlowCoordinator.GetLocalizedTitle))]
        // ReSharper disable once InconsistentNaming
        private bool PrefixGameServerLobbyFlowCoordinatorGetLocalizedTitle(ref string __result)
        {
            if (_serverInfo == null)
                // We are not managing this connection - patch does not apply
                return true;
            
            __result = _serverInfo.ServerName;
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLevelSelectionFlowCoordinator),
            nameof(MultiplayerLevelSelectionFlowCoordinator.enableCustomLevels), AffinityMethodType.Getter)]
        [AffinityAfter("com.goobwabber.multiplayercore.affinity")]
        [AffinityPriority(1)]
        // ReSharper disable twice InconsistentNaming
        private bool PrefixCustomLevelsEnabled(ref bool __result, SongPackMask ____songPackMask)
        {
            // MultiplayerCore requires an override API server to be set for custom songs to be enabled
            // We have to take over that job here if direct connecting

            if (_serverInfo is not { ConnectionMethod: ServerRepository.ConnectionMethod.DirectConnect })
                // We are not managing this connection, or it is a regular GameLift connection - patch does not apply
                return true;

            __result = ____songPackMask.Contains(new SongPackMask("custom_levelpack_CustomLevels"));
            return false;
        }

        #endregion

        #region Menu Lights

        private MenuLightsPresetSO BakeMenuLightsPreset(Color color)
        {
            var colorSo = ScriptableObject.CreateInstance<SimpleColorSO>();
            colorSo.SetColor(color);
            
            var presetSo = Instantiate(_menuLightsManager._defaultPreset);
            presetSo._playersPlaceNeonsColor = colorSo;
            foreach (var pair in presetSo._lightIdColorPairs)
            {
                pair.baseColor = colorSo;
                pair.intensity = 1f;
            }
            return presetSo;
        }

        private MenuLightsPresetSO? _menuLightsErrorPresetCached = null;
        private MenuLightsPresetSO MenuLightsErrorPreset
        {
            get
            {
                if (_menuLightsErrorPresetCached == null)
                    _menuLightsErrorPresetCached = BakeMenuLightsPreset(BssbColors.FailureRed);
                return _menuLightsErrorPresetCached;
            }
        }

        private MenuLightsPresetSO? _menuLightsConnectPresetCached = null;
        private MenuLightsPresetSO MenuLightsConnectPreset
        {
            get
            {
                if (_menuLightsConnectPresetCached == null)
                    _menuLightsConnectPresetCached = BakeMenuLightsPreset(BssbColors.GoingToAnotherDimension);
                return _menuLightsConnectPresetCached;
            }
        }

        private void SetMenuLights(MenuLightsPresetSO preset, bool animated = true) =>
            _menuLightsManager.SetColorPreset(preset, animated);

        private void ResetMenuLights(bool animated = true) =>
            SetMenuLights(_menuLightsManager._defaultPreset, animated);

        #endregion
    }
}