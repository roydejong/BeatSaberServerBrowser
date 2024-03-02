using System.Text;
using System.Threading.Tasks;
using BeatSaber.AvatarCore;
using BGLib.Polyglot;
using HMUI;
using ServerBrowser.UI.Browser;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Integrators
{
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    /// Hijacks the "Multiplayer" menu button to use our menu instead of vanilla mode selection.
    /// </summary>
    public class MainMenuIntegrator : IAffinity
    {
        [Inject] private readonly BssbConfig _bssbConfig = null!;
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BrowserFlowCoordinator _browserFlowCoordinator = null!;
        
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly MainMenuViewController _mainMenuViewController = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;
        [Inject] private readonly AvatarSystemCollection _avatarSystemCollection = null!;
        [Inject] private readonly SimpleDialogPromptViewController _simpleDialogPromptViewController = null!;

        private FlowCoordinator? _editAvatarFlowCoordinator = null;
        private bool _isDisclaimerVisible = false;
        
        public const int PrivacyDisclaimerVersion = 1;
        public const string PrivacyDisclaimerText = "With the Server Browser installed, your multiplayer games and " +
                                                    "activity will be shared with BSSB. You can view the BSSB privacy " +
                                                    "policy at https://bssb.app/privacy";

        /// <summary>
        /// Gets whether the privacy disclaimer should be shown prior to launching multiplayer.
        /// </summary>
        public bool ShouldShowDisclaimer => _bssbConfig.AcceptedPrivacyDisclaimerVersion < PrivacyDisclaimerVersion ||
                                            !_playerDataModel.playerData.agreedToMultiplayerDisclaimer;

        /// <summary>
        /// Patch: Intercept the normal main menu button handler.
        /// </summary>
        [AffinityPatch(typeof(MainMenuViewController), nameof(MainMenuViewController.HandleMenuButton))]
        [AffinityPrefix]
        public bool PrefixHandleMenuButton(MainMenuViewController.MenuButton menuButton)
        {
            if (menuButton != MainMenuViewController.MenuButton.Multiplayer)
                // Ignore all other buttons
                return true;

            _isDisclaimerVisible = false;
            
            // If needed, show multiplayer / privacy disclaimer
            if (ShouldShowDisclaimer)
            {
                _ = ShowDisclaimer();
                return false;
            }

            // Normal flow: redirect to avatar creation if needed, then launch multiplayer
            _ = CheckAvatarsAndLaunchMultiplayer();
            return false;
        }
        
        /// <summary>
        /// Patch: Intercept the avatar editor finish (in case of initial avatar setup) which would launch normal flow.
        /// </summary>
        [AffinityPatch(typeof(MainFlowCoordinator), nameof(MainFlowCoordinator.HandleEditAvatarFlowCoordinatorHelperDidFinish))]
        [AffinityPrefix]
        public bool PrefixHandleEditAvatarFlowCoordinatorHelperDidFinish(FlowCoordinator flowCoordinator,
            EditAvatarFlowCoordinatorHelper.FinishAction finishAction)
        {
            if (!_mainFlowCoordinator._goToMultiplayerAfterAvatarCreation)
                // Our patch is not applicable, no multiplayer redirect is happening
                return true;

            _mainFlowCoordinator._goToMultiplayerAfterAvatarCreation = false;
            
            _editAvatarFlowCoordinator = flowCoordinator;
            
            var dismiss = finishAction == EditAvatarFlowCoordinatorHelper.FinishAction.Back;
            _ = CheckAvatarsAndLaunchMultiplayer(true, dismiss);
            
            return false;
        }
        
        /// <summary>
        /// UI: Show the privacy disclaimer (simple dialog prompt).
        /// </summary>
        public async Task ShowDisclaimer()
        {
            _log.Info($"Showing multiplayer / privacy disclaimer (disclaimer version {PrivacyDisclaimerVersion}).");

            var bssbDisclaimer = new StringBuilder();
            bssbDisclaimer.Append("<color=#e5a300>");
            bssbDisclaimer.Append(PrivacyDisclaimerText);
            bssbDisclaimer.Append("</color>");
            
            _simpleDialogPromptViewController.Init
            (
                title: Localization.Get("LABEL_MULTIPLAYER_DISCLAIMER"),
                message: Localization.Get("MULTIPLAYER_DISCLAIMER") + "\r\n\r\n" + bssbDisclaimer,
                firstButtonText: Localization.Get("BUTTON_AGREE"),
                secondButtonText: Localization.Get("BUTTON_DO_NOT_AGREE_AND_QUIT"),
                didFinishAction: (int buttonNumber) =>
                {
                    if (buttonNumber == 0)
                    {
                        // Accept - continue to avatar or multiplayer
                        _log.Info("User accepted multiplayer disclaimer.");
                        
                        _bssbConfig.AcceptedPrivacyDisclaimerVersion = PrivacyDisclaimerVersion;
                        _playerDataModel.playerData.agreedToMultiplayerDisclaimer = true;

                        _ = CheckAvatarsAndLaunchMultiplayer(); // next step will replace our prompt view controller
                    }
                    else
                    {
                        // Decline - dismiss prompt, returning to main menu
                        _log.Info("User declined multiplayer disclaimer, quitting to main menu..");
                        _mainFlowCoordinator.DismissViewController(_simpleDialogPromptViewController);
                        _isDisclaimerVisible = false;
                    }
                }
            );
            _mainFlowCoordinator.PresentViewController(_simpleDialogPromptViewController);
            _isDisclaimerVisible = true;
        }

        /// <summary>
        /// UI: Launch multiplayer, redirecting to avatar creation first if needed.
        /// Will also be called after avatar editing completion (isCallback will be set).
        /// </summary>
        public async Task CheckAvatarsAndLaunchMultiplayer(bool isCallback = false, bool dismiss = false)
        {
            var hasAvatarSetup = await FlowCoordinatorAvatarsHelper.HasUserSelectedAvatarSystemWithCreatedAvatar(
                    _avatarSystemCollection, _playerDataModel);

            if (isCallback && (!hasAvatarSetup || dismiss))
            {
                // Callback result: avatar setup failed or user manually cancelled
                _log.Info("Avatar set up failed or cancelled by user, quitting to main menu.");
                if (_editAvatarFlowCoordinator != null)
                    _mainFlowCoordinator.DismissFlowCoordinator(_editAvatarFlowCoordinator);
                _editAvatarFlowCoordinator = null;
                return;
            }

            if (hasAvatarSetup)
            {
                _ = LaunchMultiplayer(afterAvatarEdit: isCallback);
                return;
            }

            _log.Info("User has not yet set up an avatar, redirecting to avatar creation.");
            _mainFlowCoordinator._goToMultiplayerAfterAvatarCreation = true;
            _mainFlowCoordinator._editAvatarFlowCoordinatorHelper.Show(_mainFlowCoordinator, true,
                replaceTopViewController: true);
            _isDisclaimerVisible = false; // replaced by avatar creation
        }

        public async Task LaunchMultiplayer(bool afterAvatarEdit = false)
        {
            if (afterAvatarEdit)
            {
                _mainFlowCoordinator.ReplaceChildFlowCoordinator(_browserFlowCoordinator);
            }
            else
            {
                _mainFlowCoordinator.PresentFlowCoordinator(_browserFlowCoordinator,
                    replaceTopViewController: _isDisclaimerVisible);
                _isDisclaimerVisible = false;
            }
        }
    }
}