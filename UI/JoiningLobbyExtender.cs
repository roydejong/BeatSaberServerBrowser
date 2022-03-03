using BGNet.Core.GameLift;
using IPA.Utilities;
using Polyglot;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.UI
{
    /// <summary>
    /// Extends the "Joining lobby" view with actually useful information.
    /// </summary>
    public class JoiningLobbyExtender : IInitializable, IAffinity
    {
        private const string LocalizationKeyJoiningLobby = "LABEL_JOINING_LOBBY";
        private const string LocalizationKeyJoiningGame = "LABEL_JOINING_GAME";
        private const string LocalizationKeyCreatingServer = "LABEL_CREATING_SERVER";
        private const string LocalizationKeyJoiningQuickPlay = "LABEL_JOINING_QUICK_PLAY";

        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly JoiningLobbyViewController _viewController = null!;
        [Inject] private readonly IMultiplayerSessionManager _sessionManager = null!;

        private bool _weAreHandling;
        private bool _isQuickPlay;
        private string? _originalText;
        private string? _starterText;

        public void Initialize()
        {
            _viewController.didActivateEvent += HandleViewDidActivate;
        }

        #region View events

        [AffinityPostfix]
        [AffinityPatch(typeof(JoiningLobbyViewController), "Init")]
        private void HandleViewInit(string text)
        {
            if (text == Localization.Get(LocalizationKeyJoiningLobby)
                || text == Localization.Get(LocalizationKeyJoiningGame)
                || text == Localization.Get(LocalizationKeyCreatingServer)
                || text == Localization.Get(LocalizationKeyJoiningQuickPlay))
            {
                _weAreHandling = true;
                _isQuickPlay = (text == Localization.Get(LocalizationKeyJoiningQuickPlay));
                _originalText = text;
            }
            else
            {
                _weAreHandling = false;
            }
        }

        private void HandleViewDidActivate(bool firstactivation, bool addedtohierarchy, bool screensystemenabling)
        {
            if (_weAreHandling)
            {
                SetText(_starterText ?? "Connecting to master server...");
            }
        }

        #endregion

        #region Connection events

        [AffinityPrefix]
        [AffinityPatch(typeof(MasterServerConnectionManager), "MasterServerConnectToServer")]
        private void HandleMasterServerConnectToServer()
        {
            // Regular master server: connect
            _starterText = "Connecting to master server...";

            SetText(_starterText);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameLiftPlayerSessionProvider), "GetGameLiftPlayerSessionInfo")]
        private void HandleGetGameLiftPlayerSessionInfo(GameplayServerConfiguration gameplayServerConfiguration)
        {
            // GameLift API: Request a multiplayer server instance
            // This seems to happen before the loading view inits
            if (gameplayServerConfiguration.gameplayServerMode == GameplayServerMode.Countdown) // Quick Play
                _starterText = "Looking for players...";
            else
                _starterText = "Requesting server instance...";

            SetText(_starterText);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameLiftClientMessageHandler), "AuthenticateWithGameLiftServer")]
        private void HandleAuthenticateWithGameLiftServer()
        {
            if (!_weAreHandling)
                return;

            // GameLift master server connection starting
            SetText("Connecting to master server...");
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MasterServerConnectionManager), "HandleConnectToServerSuccess")]
        private void HandleMasterServerPreConnect()
        {
            if (!_weAreHandling)
                return;

            // Normal master server connected; gameplay server info received
            SetText("Connecting to game server...");
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameLiftConnectionManager), "HandleConnectToServerSuccess")]
        private void HandleGameLiftPreConnect()
        {
            if (!_weAreHandling)
                return;

            // GameLift master server connected; gameplay server info received
            SetText("Connecting to game server...");
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerSessionManager), "UpdateConnectionState")]
        private void HandleUpdateSessionConnectionState(UpdateConnectionStateReason updateReason)
        {
            if (!_weAreHandling)
                return;

            switch (updateReason)
            {
                case UpdateConnectionStateReason.SyncTimeInitialized:
                    // We are connected to the game server, and are about to enter the lobby
                    SetText("Entering lobby...");
                    break;
            }
        }

        #endregion

        #region View utils

        private string GetCurrentText() => _viewController.GetField<string, JoiningLobbyViewController>("_text");

        private void SetText(string text)
        {
            if (GetCurrentText() == text)
                return;

            _log.Info($"Extended join status: {text}");

            _viewController
                .GetField<LoadingControl, JoiningLobbyViewController>("_loadingControl")
                .ShowLoading(text);
        }

        #endregion
    }
}