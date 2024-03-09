using System;
using System.Threading;
using System.Threading.Tasks;
using HMUI;
using ServerBrowser.Data;
using ServerBrowser.Session;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.Util;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public partial class MainBrowserViewController : ViewController, IInitializable, IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbSession _session = null!;
        [Inject] private readonly ServerRepository _serverRepository = null!;
        
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;

        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;

        public void Initialize()
        {
            BuildLayout(_layoutBuilder.Init(this));
            
            _loadingControl!.ShowLoading("Loading Servers");
            
            _session.LocalUserInfoChangedEvent += HandleLocalUserInfoUpdated;
            _session.AvatarUrlChangedEvent += HandleAvatarUrlChanged;
            _session.LoginStatusChangedEvent += HandleLoginStatusChanged;
            
            _serverRepository.ServersUpdatedEvent += HandleServersUpdated;
            _serverRepository.RefreshFinishedEvent += HandleServersRefreshFinished;
            
            if (_session.LocalUserInfo != null)
                HandleLocalUserInfoUpdated(_session.LocalUserInfo);
            else
                SetLocalUserInfoEmpty();
            
            HandleLoginStatusChanged(_session.IsLoggedIn);
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            _scrollView!.Refresh(true);
            Task.Run(async () =>
            {
                // TODO Figure out why this is needed and get rid of it
                await Task.Delay(100);
                _scrollView!.Refresh(true);
            });
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        public void Dispose()
        {
            _session.LocalUserInfoChangedEvent -= HandleLocalUserInfoUpdated;
            _session.AvatarUrlChangedEvent -= HandleAvatarUrlChanged;
            _session.LoginStatusChangedEvent -= HandleLoginStatusChanged;
            
            _serverRepository.ServersUpdatedEvent -= HandleServersUpdated;
            _serverRepository.RefreshFinishedEvent -= HandleServersRefreshFinished;
        }

        private void SetLocalUserInfoEmpty()
        {
            _selfUsernameText!.SetText($"No user info");
            _selfUsernameText.SetTextColor(BssbColors.InactiveGray);
            
            _ = _selfAvatarImage!.SetPlaceholderAvatar(CancellationToken.None);
        }
        
        private void HandleLocalUserInfoUpdated(UserInfo userInfo)
        {
            _log.Info($"User info updated: {userInfo}");

            var username = userInfo.userName.StripTags();
            _selfUsernameText!.SetText($"{username}");
            _selfUsernameText.SetTextColor(BssbColors.White);
        }
        
        private void HandleAvatarUrlChanged(string? avatarUrl)
        {
            _log.Info($"Avatar URL updated: {avatarUrl}");
            _ = _selfAvatarImage!.SetAvatarFromUrl(avatarUrl, CancellationToken.None);
        }

        private void HandleLoginStatusChanged(bool loggedIn)
        {
            _log.Info($"Login status changed: {loggedIn}");

            if (!loggedIn)
            {
                // BSSB login failed
                _selfUsernameText!.SetText("Login failed");
                _selfUsernameText.SetTextColor(BssbColors.Orange);
            }
            else if (_session.LocalUserInfo != null)
            {
                // BSSB login success; just presenting local user info for now
                HandleLocalUserInfoUpdated(_session.LocalUserInfo);
            }
        }
        
        private void HandleServersUpdated()
        {
            _log.Info($"Servers updated");
            
            // TODO Create / update cells for servers

            if (_serverRepository.Servers.Count == 0)
            {
                _loadingControl!.ShowLoading("Loading Servers");
                _scrollView!.Refresh(true);
            }
            else
            {
                _loadingControl!.Hide();
                _scrollView!.Refresh(false);
            }
        }
        
        private void HandleServersRefreshFinished()
        {
            _log.Info($"Servers refresh finished");

            if (_serverRepository.Servers.Count == 0)
            {
                _loadingControl!.ShowText("No servers found", true);
                _scrollView!.Refresh(true);
            }
        }
        
        private void HandleQuickPlayClicked()
        {
            _log.Info($"Quick Play");
        }

        private void HandleCreateServerClicked()
        {
            _log.Info($"Create Server");
        }
        
        private void HandleJoinByCodeClicked()
        {
            _log.Info($"Join by Code");
        }

        private void HandleEditAvatarClicked()
        {
            _mainFlowCoordinator._goToMultiplayerAfterAvatarCreation = true;
            _mainFlowCoordinator._editAvatarFlowCoordinatorHelper.Show(_mainFlowCoordinator.childFlowCoordinator, true);
        }
        
        private void HandleSearchInputChanged(InputFieldView.SelectionState state, string value)
        {
            _log.Info($"Search updated: {state}, \"{value}\"");
        }
        
        private void HandleFilterButtonClicked()
        {
            _log.Info($"Filters clicked");
            _filterButton!.SetTextValue("ooh ya clicked me good");
        }
        
        private void HandleFilterButtonCleared()
        {
            _log.Info($"Filters cleared");
        }
        
        private void HandleRefreshClicked()
        {
            _log.Info($"Refresh clicked");
            _loadingControl!.ShowLoading("Loading Servers");
            _serverRepository.StartDiscovery();
            // So yeah this button basically does nothing, but it'll definitely spin 'til next auto refresh :)
        }
    }
}