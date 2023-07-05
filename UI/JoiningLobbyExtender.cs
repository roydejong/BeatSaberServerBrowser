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
    public class JoiningLobbyExtender : IAffinity
    {
        private const string LocalizationKeyJoiningLobby = "LABEL_JOINING_LOBBY";
        private const string LocalizationKeyJoiningGame = "LABEL_JOINING_GAME";
        private const string LocalizationKeyCreatingServer = "LABEL_CREATING_SERVER";
        private const string LocalizationKeyJoiningQuickPlay = "LABEL_JOINING_QUICK_PLAY";

        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly JoiningLobbyViewController _viewController = null!;

        private bool _weAreHandling;

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
            }
            else
            {
                _weAreHandling = false;
            }
        }

        #endregion

        #region Connection events

        [AffinityPrefix]
        [AffinityPatch(typeof(GameLiftConnectionManager), "HandleConnectToServerSuccess")]
        private void HandleConnectToServerSuccess()
        {
            if (!_weAreHandling)
                return;

            SetText("Connecting to game...");
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

        private string? GetCurrentText() => _viewController.GetField<string, JoiningLobbyViewController>("_text");

        private void SetText(string text)
        {
            if (GetCurrentText() == text)
                return;

            var loadingControl = _viewController.GetField<LoadingControl, JoiningLobbyViewController>("_loadingControl");
            if (loadingControl == null)
                return;

            _log.Debug($"Extended join status: {text}");
            loadingControl.ShowLoading(text);
        }

        #endregion
    }
}