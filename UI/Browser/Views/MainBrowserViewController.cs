using System;
using System.Collections.Generic;
using System.Linq;
using HMUI;
using ServerBrowser.Data;
using ServerBrowser.Models;
using ServerBrowser.UI.Browser.Modals;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.UI.Toolkit.Modals;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public partial class MainBrowserViewController : ViewController, IInitializable, IDisposable
    {
        [Inject] private readonly DiContainer _diContainer = null!;
        [Inject] private readonly BssbSession _session = null!;
        [Inject] private readonly ServerRepository _serverRepository = null!;
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;
        
        private readonly List<TkServerCell> _serverCells = new();
        private bool _completedFullRefresh = false;
        private int _lastContentHeight = 0;
        private AccountModalView? _accountModalView = null;
        
        public event Action<ServerRepository.ServerInfo>? ServerJoinRequestedEvent;
        public event Action<MultiplayerModeSelectionViewController.MenuButton>? ModeSelectedEvent;
        public event Action? AvatarEditRequestedEvent;
        public event Action? FiltersClickedEvent;
        public event Action? FiltersClearedEvent;

        #region Init / Deinit
        
        public void Initialize()
        {
            BuildLayout(_layoutBuilder.Init(this));
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            if (addedToHierarchy)
            {
                _session.LocalUserInfoChangedEvent += HandleLocalUserInfoUpdated;
                _session.AvatarUrlChangedEvent += HandleAvatarUrlChanged;
                _session.LoginStatusChangedEvent += HandleLoginStatusChanged;

                _serverRepository.ServersUpdatedEvent += HandleServersUpdated;
                _serverRepository.RefreshFinishedEvent += HandleServersRefreshFinished;
            }

            RefreshAccountStatus();
            
            _serverRepository.StartDiscovery(); // ensure we restart, in case we came back from lobby / connection error
            HandleServersUpdated(_serverRepository.FilteredServers);
            
            RefreshLoadingState();
            _completedFullRefresh = false;
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);

            if (removedFromHierarchy)
            {
                _session.LocalUserInfoChangedEvent -= HandleLocalUserInfoUpdated;
                _session.AvatarUrlChangedEvent -= HandleAvatarUrlChanged;
                _session.LoginStatusChangedEvent -= HandleLoginStatusChanged;

                _serverRepository.ServersUpdatedEvent -= HandleServersUpdated;
                _serverRepository.RefreshFinishedEvent -= HandleServersRefreshFinished;
            }

            TkModalHost.CloseAnyModal(this);
        }

        public void Dispose()
        {
        }

        #endregion

        #region Session / Account
        
        private void HandleLocalUserInfoUpdated(UserInfo userInfo)
        {
            RefreshAccountStatus();
        }
        
        private void HandleAvatarUrlChanged(string? avatarUrl)
        {
            RefreshAccountStatus();
        }

        private void HandleLoginStatusChanged(bool loggedIn)
        {
            RefreshAccountStatus();
        }

        private void RefreshAccountStatus()
        {
            if (_accountModalView != null)
                _accountModalView.SetData(_session.LocalUserInfo, _session.IsLoggedIn);
            
            if (_session.LocalUserInfo == null)
            {
                _accountTile.SetNoLocalUserInfo();
                return;
            }

            if (!_session.IsLoggedIn)
            {
                if (_session.AttemptingLogin)
                    _accountTile.SetLoggingIn(_session.LocalUserInfo.userName);
                else
                    _accountTile.SetLoginFailed(_session.LocalUserInfo.userName);
                return;
            }

            _accountTile.SetLoggedIn(_session.LocalUserInfo.userName, _session.AvatarUrl);
        }

        private void HandleAccountTileClicked()
        {
            _accountModalView = TkModalHost.ShowModal<AccountModalView>(this, _diContainer);
            RefreshAccountStatus();
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
                cell.ClickedEvent += HandleServerCellClicked;
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

        private void HandleServerCellClicked(ServerRepository.ServerInfo serverInfo)
        {
            var modal = TkModalHost.ShowModal<ServerModalView>(this, _diContainer);
            modal.SetData(serverInfo);
            modal.ConnectClickedEvent += HandleServerConnectClicked;
        }

        private void HandleServerConnectClicked(ServerRepository.ServerInfo serverInfo)
        {
            TkModalHost.CloseAnyModal(this);
            ServerJoinRequestedEvent?.Invoke(serverInfo);
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
            ModeSelectedEvent?.Invoke(MultiplayerModeSelectionViewController.MenuButton.QuickPlay);
        }

        private void HandleCreateServerClicked()
        {
            ModeSelectedEvent?.Invoke(MultiplayerModeSelectionViewController.MenuButton.CreateServer);
        }
        
        private void HandleJoinByCodeClicked()
        {
            ModeSelectedEvent?.Invoke(MultiplayerModeSelectionViewController.MenuButton.JoinWithCode);
        }

        private void HandleEditAvatarClicked()
        {
            AvatarEditRequestedEvent?.Invoke();
        }
        
        #endregion

        #region Search & Filter
        
        public void UpdateFiltersValue(ServerFilterParams filterParams)
        {
            _filterButton!.SetTextValue(filterParams.Describe());
        }
        
        private void HandleTextInputChanged(InputFieldView.SelectionState state, string value)
        {
            if (_serverRepository.FilterText == value)
                return;
            
            _serverRepository.SetFilterText(value);
        }
        
        private void HandleFilterButtonClicked()
        {
            FiltersClickedEvent?.Invoke();
        }
        
        private void HandleFilterButtonCleared()
        {
            FiltersClearedEvent?.Invoke();
        }
        
        #endregion
    }
}