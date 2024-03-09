using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HMUI;
using ServerBrowser.Data;
using ServerBrowser.Session;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Wrappers;
using ServerBrowser.Util;
using SiraUtil.Logging;
using UnityEngine;
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
        
        private List<TkButton> _serverCells = new();
        private bool _completedFullRefresh = false;
        private int _lastContentHeight = 0;

        #region Init / Deinit
        
        public void Initialize()
        {
            BuildLayout(_layoutBuilder.Init(this));
            
            _session.LocalUserInfoChangedEvent += HandleLocalUserInfoUpdated;
            _session.AvatarUrlChangedEvent += HandleAvatarUrlChanged;
            _session.LoginStatusChangedEvent += HandleLoginStatusChanged;
            
            _serverRepository.ServersUpdatedEvent += HandleServersUpdated;
            _serverRepository.RefreshFinishedEvent += HandleServersRefreshFinished;
            
            if (_session.LocalUserInfo != null)
                HandleLocalUserInfoUpdated(_session.LocalUserInfo);
            else
                SetLocalUserInfoEmpty();
            
            if (_session.IsLoggedIn)
                HandleLoginStatusChanged(true);
            
            RefreshLoadingState();
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            _completedFullRefresh = false;
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

        #endregion

        #region Session

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
        
        #endregion

        #region Server List
        
        private void HandleServersUpdated(IReadOnlyCollection<ServerRepository.ServerInfo> servers)
        {
            _log.Info($"Servers updated: have {servers.Count}");

            var container = _scrollView!.Content!;
            var containerWidth = container.RectTransform.rect.width;

            const int cellsPerRow = 3;
            var cellWidth = containerWidth / cellsPerRow;
            const float cellHeight = 20f;
            
            var targetCellCount = servers.Count;
            var currentCellCount = _serverCells.Count;
            var extraCellsNeeded = servers.Count - currentCellCount;
            var excessCells = currentCellCount - servers.Count;
            
            var columnCount = Mathf.FloorToInt(containerWidth / cellWidth);
            var rowCount = Mathf.CeilToInt((float)targetCellCount / (float)columnCount);
            
            // Initialize new cells
            for (var i = 0; i < extraCellsNeeded; i++)
            {
                // TODO Not a button but a real UI component
                
                var cell = container.AddButton("ServerCell");
                cell.SetHeight(cellHeight);
                cell.SetWidth(cellWidth);
                cell.DisableSkew();
                _serverCells.Add(cell);
                
                var column = i % columnCount;
                var row = i / columnCount;

                var cellRect = (cell.GameObject.transform as RectTransform)!;
                cellRect.anchorMin = new Vector2(0f, 1f);
                cellRect.anchorMax = new Vector2(0f, 1f);
                cellRect.pivot = new Vector2(0f, 1f);
                cellRect.anchoredPosition = new Vector2((float)column * cellWidth, (float)row * -cellHeight);
            }
            
            // Sync cell contents based on server list
            for (var i = 0; i < servers.Count; i++)
            {
                var server = servers.ElementAt(i);
                var cell = _serverCells[i];
                cell.SetText(server.ServerName);
                cell.GameObject.SetActive(true);
            }
            
            // Disable (but do not remove) excess cells
            for (var i = 0; i < excessCells; i++)
            {
                var cell = _serverCells[currentCellCount - i - 1];
                cell.GameObject.SetActive(false);
            }

            // Toggle loader
            RefreshLoadingState();
            
            // Manual scroll view content height
            const float viewportHeight = 57f;
            var newContentHeight = Mathf.CeilToInt(rowCount * cellHeight);
            if (newContentHeight < viewportHeight)
                newContentHeight = (int)viewportHeight;
            if (newContentHeight != _lastContentHeight)
            {
                _scrollView!.SetContentHeight(newContentHeight - viewportHeight); // will adjust our content's delta size
                _lastContentHeight = newContentHeight;
            }
        }
        
        private void HandleServersRefreshFinished()
        {
            _completedFullRefresh = true; 
            RefreshLoadingState();
        }
        
        private void HandleRefreshClicked()
        {
            _completedFullRefresh = false;
            RefreshLoadingState();
            // So yeah this button basically does nothing, but it'll definitely spin 'til next full refresh :)
        }

        private void RefreshLoadingState()
        {
            if (_serverRepository.NoResults)
            {
                if (_completedFullRefresh)
                {
                    _loadingControl!.ShowText("No servers found", true);
                }
                else
                {
                    _loadingControl!.ShowLoading("Loading Servers");
                }
            }
            else
            {
                _loadingControl!.Hide();
            }
        }
        
        #endregion

        #region Mode Selection
        
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
        
        #endregion

        #region Search & Filter
        
        private void HandleSearchInputChanged(InputFieldView.SelectionState state, string value)
        {
            if (_serverRepository.FilterText == value)
                return;
            
            _serverRepository.SetFilterText(value);
        }
        
        private void HandleFilterButtonClicked()
        {
            // TODO Filter params impl
            _filterButton!.SetTextValue("ooh ya clicked me good");
        }
        
        private void HandleFilterButtonCleared()
        {
            // TODO Filter params impl
        }
        
        #endregion
    }
}