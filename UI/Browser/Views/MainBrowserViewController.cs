using System;
using System.Threading;
using HMUI;
using ServerBrowser.Session;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.Util;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public partial class MainBrowserViewController : ViewController, IInitializable, IDisposable
    {
        [Inject] private readonly BssbSession _session = null!;
        
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;

        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;

        public void Initialize()
        {
            BuildLayout(_layoutBuilder.Init(this));
            
            _session.UserInfoChangedEvent += HandleUserInfoUpdated;
            _session.AvatarUrlChangedEvent += HandleAvatarUrlChanged;
            
            if (_session.UserInfo != null)
                HandleUserInfoUpdated(_session.UserInfo);
            else
                SetUserInfoNotLoggedIn();
        }

        public void Dispose()
        {
            _session.UserInfoChangedEvent -= HandleUserInfoUpdated;
            _session.AvatarUrlChangedEvent -= HandleAvatarUrlChanged;
        }

        private void SetUserInfoNotLoggedIn()
        {
            _selfUsernameText!.SetText($"Offline");
            _selfUsernameText.SetTextColor(BssbColors.InactiveGray);
            
            _ = _selfAvatarImage!.SetPlaceholderAvatar(CancellationToken.None);
        }
        
        private void HandleUserInfoUpdated(UserInfo userInfo)
        {
            Plugin.Log.Error($"User info updated: {userInfo}");

            var username = userInfo.userName.StripTags();
            _selfUsernameText!.SetText($"{username}");
            _selfUsernameText.SetTextColor(BssbColors.White);
        }
        
        private void HandleAvatarUrlChanged(string? avatarUrl)
        {
            Plugin.Log.Error($"Avatar URL updated: {avatarUrl}");
            _ = _selfAvatarImage!.SetAvatarFromUrl(avatarUrl, CancellationToken.None);
        }
        
        private void HandleQuickPlayClicked()
        {
            Plugin.Log.Error($"Quick Play");
        }

        private void HandleCreateServerClicked()
        {
            Plugin.Log.Error($"Create Server");
        }
        
        private void HandleJoinByCodeClicked()
        {
            Plugin.Log.Error($"Join by Code");
        }

        private void HandleEditAvatarClicked()
        {
            _mainFlowCoordinator._goToMultiplayerAfterAvatarCreation = true;
            _mainFlowCoordinator._editAvatarFlowCoordinatorHelper.Show(_mainFlowCoordinator.childFlowCoordinator, true);
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