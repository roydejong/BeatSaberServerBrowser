using System;
using System.Collections.Generic;
using System.Linq;
using HMUI;
using ServerBrowser.Data;
using ServerBrowser.Session;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.Util;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public partial class MainBrowserViewController : ViewController, IInitializable, IDisposable
    {
        [Inject] private readonly BssbSession _session = null!;
        [Inject] private readonly ServerRepository _serverRepository = null!;
        
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;
        
        private readonly List<TkServerCell> _serverCells = new();
        private bool _completedFullRefresh = false;
        private int _lastContentHeight = 0;

        #region Init / Deinit
        
        public void Initialize()
        {
            BuildLayout(_layoutBuilder.Init(this));
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
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
            
            HandleAvatarUrlChanged(_session.AvatarUrl);
            
            RefreshLoadingState();

            _completedFullRefresh = false;
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            
            _session.LocalUserInfoChangedEvent -= HandleLocalUserInfoUpdated;
            _session.AvatarUrlChangedEvent -= HandleAvatarUrlChanged;
            _session.LoginStatusChangedEvent -= HandleLoginStatusChanged;
            
            _serverRepository.ServersUpdatedEvent -= HandleServersUpdated;
            _serverRepository.RefreshFinishedEvent -= HandleServersRefreshFinished;
        }

        public void Dispose()
        {
        }

        #endregion

        #region Session

        private void SetLocalUserInfoEmpty()
        {
            _selfUsernameText!.SetText($"No user info");
            _selfUsernameText.SetTextColor(BssbColors.InactiveGray);
            
            _ = _selfAvatarImage!.SetPlaceholderAvatar();
        }
        
        private void HandleLocalUserInfoUpdated(UserInfo userInfo)
        {
            var username = userInfo.userName.StripTags();
            _selfUsernameText!.SetText($"{username}");
            _selfUsernameText.SetTextColor(BssbColors.White);
        }
        
        private void HandleAvatarUrlChanged(string? avatarUrl)
        {
            Plugin.Log.Info($"HandleAvatarUrlChanged: {avatarUrl}");
            _ = string.IsNullOrWhiteSpace(avatarUrl)
                ? _selfAvatarImage!.SetPlaceholderAvatar()
                : _selfAvatarImage!.SetRemoteImage(avatarUrl);
        }

        private void HandleLoginStatusChanged(bool loggedIn)
        {
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
        
        public const float CellHeight = 20.333333f;
        public const float CellsPerRow = 2;

        private float _lastCellWidth = 0;
        
        private void HandleServersUpdated(IReadOnlyCollection<ServerRepository.ServerInfo> servers)
        {
            var container = _scrollView!.Content!;
            var containerWidth = container.RectTransform.rect.width;
            
            var cellWidth = containerWidth / CellsPerRow;
            
            var targetCellCount = servers.Count;
            var currentCellCount = _serverCells.Count;
            var extraCellsNeeded = servers.Count - currentCellCount;
            var excessCells = currentCellCount - servers.Count;
            
            var columnCount = Mathf.FloorToInt(containerWidth / cellWidth);
            var rowCount = Mathf.CeilToInt((float)targetCellCount / (float)columnCount);
            
            // Unfortunately, sometimes the rect isn't fully sized out yet when we do this, so we may need to resize
            var shouldResizeCells = Mathf.Approximately(cellWidth, _lastCellWidth);
            _lastCellWidth = cellWidth;
            
            // Initialize new cells
            for (var i = 0; i < extraCellsNeeded; i++)
            {
                var cell = container.AddServerCell();
                _serverCells.Add(cell);
                shouldResizeCells = true;
            }
            
            // Sync cell contents based on server list
            for (var i = 0; i < servers.Count; i++)
            {
                var server = servers.ElementAt(i);
                var cell = _serverCells[i];
                if (shouldResizeCells || i >= currentCellCount)
                {
                    var column = i % columnCount;
                    var row = i / columnCount;
                    cell.SetSize(cellWidth, CellHeight);
                    cell.SetPosition(column * cellWidth, row * -CellHeight);
                }
                cell.SetData(server);
                cell.SetActive(true);
            }
            
            // Disable (but do not remove) excess cells
            for (var i = 0; i < excessCells; i++)
            {
                var cell = _serverCells[currentCellCount - i - 1];
                cell.SetActive(false);
            }

            // Toggle loader
            RefreshLoadingState();
            
            // Manual scroll view content height
            const float viewportHeight = 61f;
            var newContentHeight = Mathf.CeilToInt(rowCount * CellHeight);
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
        
        public event Action<ModeSelectionTarget>? ModeSelectedEvent;

        public enum ModeSelectionTarget
        {
            QuickPlay = 0,
            CreateServer = 1,
            JoinByCode = 2,
            EditAvatar = 3
        }
        
        private void HandleQuickPlayClicked()
        {
            ModeSelectedEvent?.Invoke(ModeSelectionTarget.QuickPlay);
        }

        private void HandleCreateServerClicked()
        {
            ModeSelectedEvent?.Invoke(ModeSelectionTarget.CreateServer);
        }
        
        private void HandleJoinByCodeClicked()
        {
            ModeSelectedEvent?.Invoke(ModeSelectionTarget.JoinByCode);
        }

        private void HandleEditAvatarClicked()
        {
            ModeSelectedEvent?.Invoke(ModeSelectionTarget.EditAvatar);
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