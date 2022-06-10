using System;
using System.Text;
using HMUI;
using Polyglot;
using ServerBrowser.Models;
using ServerBrowser.UI.Components;
using ServerBrowser.UI.Utils;
using ServerBrowser.UI.Views;
using ServerBrowser.Utils;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerBrowserFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly ServerBrowserMainViewController _mainViewController = null!;
        [Inject] private readonly ServerBrowserDetailViewController _detailViewController = null!;
        [Inject] private readonly MultiplayerModeSelectionFlowCoordinator _modeSelectionFlowCoordinator = null!;
        [Inject] private readonly ModeSelectionIntegrator _modeSelectionIntegrator = null!;
        [Inject] private readonly CoverArtLoader _coverArtLoader = null!;
        [Inject] private readonly BssbFloatingAlert _floatingAlert = null!;
        [Inject] private readonly SimpleDialogPromptViewController _simpleDialogPromptViewController = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _mainViewController.RefreshStartedEvent += HandleRefreshStartedEvent;
            _mainViewController.CreateServerClickedEvent += HandleCreateServerClicked;
            _mainViewController.ServerSelectedEvent += HandleServerSelected;
            _mainViewController.ConnectClickedEvent += HandleConnectClicked;
            _detailViewController.ConnectClickedEvent += HandleConnectClicked;

            if (firstActivation)
            {
                SetInteractionMode(true);

                _coverArtLoader.UnloadCache();

                ProvideInitialViewControllers(
                    mainViewController: _mainViewController,
                    rightScreenViewController: _detailViewController
                );
            }
            else
            {
                _detailViewController.ClearData();
            }

            _floatingAlert.DismissAllImmediate();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _mainViewController.RefreshStartedEvent -= HandleRefreshStartedEvent;
            _mainViewController.CreateServerClickedEvent -= HandleCreateServerClicked;
            _mainViewController.ServerSelectedEvent -= HandleServerSelected;
            _mainViewController.ConnectClickedEvent -= HandleConnectClicked;
            _detailViewController.ConnectClickedEvent -= HandleConnectClicked;

            if (removedFromHierarchy)
                _coverArtLoader.UnloadCache();

            _floatingAlert.DismissAllPending();
            _floatingAlert.DismissPinned();
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            ReturnToModeSelection();
        }

        private async void HandleRefreshStartedEvent(object sender, EventArgs e)
        {
            await _detailViewController.Refresh();
        }

        private void HandleCreateServerClicked(object sender, EventArgs e)
        {
            _config.AnnounceParty = true; // turn the announce switch on when creating server from our UI

            ReturnToModeSelection(targetButton: MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }

        private async void HandleServerSelected(object sender, BssbServer? server)
        {
            if (server?.Key != null)
                await _detailViewController.LoadDetailsAsync(server.Key);
        }

        private void HandleConnectClicked(object sender, BssbServer server)
        {
            if (!DoPreflightChecks(server))
                return;

            ReturnToModeSelection(targetServer: server);
        }

        private void ReturnToModeSelection(BssbServer? targetServer = null,
            MultiplayerModeSelectionViewController.MenuButton? targetButton = null)
        {
            if (targetServer is not null)
                _modeSelectionIntegrator.SetMasterServerOverride(targetServer);

            var finishedCallback = new Action(() =>
            {
                if (targetServer is not null)
                    _modeSelectionIntegrator.ConnectToServer(targetServer);
                else if (targetButton is not null)
                    _modeSelectionIntegrator.TriggerMenuButton(targetButton.Value);
            });

            _mainFlowCoordinator.ReplaceChildFlowCoordinator(_modeSelectionFlowCoordinator,
                finishedCallback, ViewController.AnimationDirection.Vertical, false);
        }

        #region Preflight validation/error

        private bool DoPreflightChecks(BssbServer server)
        {
            if (server.IsOfficial)
            {
                // Official server: can always connect to these on PC; mod versions not relevant
                return true;
            }

            if (server.MultiplayerCoreVersion != null &&
                server.MultiplayerCoreVersion != ModCheck.MultiplayerCore.InstalledVersion)
            {
                // MultiplayerCore mismatch
                PresentVersionMismatchError("MultiplayerCore",
                    server.MultiplayerCoreVersion,
                    ModCheck.MultiplayerCore.InstalledVersion);
                return false;
            }

            // No issues found
            return true;
        }

        private void PresentVersionMismatchError(string modName, Hive.Versioning.Version? theirVersion,
            Hive.Versioning.Version? ourVersion)
        {
            var theirVersionText = theirVersion != null ? theirVersion.ToString() : "Not installed";
            var ourVersionText = ourVersion != null ? ourVersion.ToString() : "Not installed";

            _log.Warn($"Pre-flight: blocking game join due to {modName} version mismatch " +
                      $"(theirVersion={theirVersion}, ourVersion={ourVersion})");

            var errorBody = new StringBuilder();
            errorBody.AppendLine($"{modName} version difference detected!");
            errorBody.AppendLine($"Make sure you and the host are both using the latest version.");
            errorBody.AppendLine();
            errorBody.AppendLine($"Their version: {theirVersionText}");
            errorBody.AppendLine($"Your version: {ourVersionText}");

            PresentSimpleError("Incompatible game", errorBody.ToString());
        }

        private void PresentSimpleError(string errorTitle, string errorText)
        {
            if (_simpleDialogPromptViewController.isInViewControllerHierarchy)
                // Already active
                return;

            _simpleDialogPromptViewController.Init(errorTitle, errorText, Localization.Get("BUTTON_OK"),
                (int btnId) =>
                {
                    SetInteractionMode(true);
                    ReplaceTopViewController(_mainViewController, null, ViewController.AnimationType.Out,
                        ViewController.AnimationDirection.Vertical);
                    _mainViewController.TryRestoreSelection(_detailViewController.CurrentSelection);
                });

            SetInteractionMode(false);
            ReplaceTopViewController(_simpleDialogPromptViewController, null, ViewController.AnimationType.In,
                ViewController.AnimationDirection.Vertical);
        }

        private void SetInteractionMode(bool makeInteractable)
        {
            if (makeInteractable)
            {
                SetTitle("Server Browser");
                showBackButton = true;
                SetRightScreenViewController(_detailViewController, ViewController.AnimationType.In);
            }
            else
            {
                SetTitle("");
                showBackButton = false;
                SetRightScreenViewController(null, ViewController.AnimationType.Out);
                _floatingAlert.DismissAnimated();
            }
        }

        #endregion
    }
}