using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using JetBrains.Annotations;
using ModestTree;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Models;
using ServerBrowser.UI.Components;
using ServerBrowser.UI.Utils;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Views
{
    [HotReload]
    public class ServerBrowserMainViewController : BSMLAutomaticViewController, IDisposable
    {
        [Inject] private readonly DiContainer _di = null!;
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly BssbBrowser _browser = null!;
        [Inject] private readonly BssbFloatingAlert _floatingAlert = null!;

        [UIParams] private readonly BSMLParserParams _parserParams = null!;

        [UIComponent("mainContentRoot")] private readonly VerticalLayoutGroup _mainContentRoot = null!;
        [UIComponent("refreshButton")] private readonly Button _refreshButton = null!;
        [UIComponent("filterButton")] private readonly Button _filterButton = null!;
        [UIComponent("filterFull")] private readonly Button _filterSubButtonFull = null!;
        [UIComponent("filterInProgress")] private readonly Button _filterSubButtonInProgress = null!;
        [UIComponent("filterVanilla")] private readonly Button _filterSubButtonVanilla = null!;
        [UIComponent("filterQuickPlay")] private readonly Button _filterSubButtonQuickPlay = null!;
        [UIComponent("createButton")] private readonly Button _createButton = null!;
        [UIComponent("connectButton")] private readonly Button _connectButton = null!;
        [UIComponent("scrollIndicator")] private readonly VerticalScrollIndicator _scrollIndicator = null!;
        [UIComponent("pageUpButton")] private readonly Button _pageUpButton = null!;
        [UIComponent("pageDownButton")] private readonly Button _pageDownButton = null!;
        [UIComponent("serverList")] private readonly CustomListTableData _serverList = null!;
        [UIComponent("paginatorText")] private readonly FormattableText _paginatorText = null!;

        private bool _bsmlReady;
        private LoadingControl? _loadingControl;
        private BssbServer? _selectedServer;
        private bool _selectionValid;
        private CancellationTokenSource? _coverArtCts;
        
        private ScrollView? _scrollView;
        private float _lastScrollPosApprox;
        private float _lastScrollPosExact;
        private float _restoreScrollPos;

        private bool _refreshPending;
        private float _lastRefreshTime;
        
        public event EventHandler<EventArgs>? RefreshStartedEvent;
        public event EventHandler<EventArgs>? CreateServerClickedEvent;
        public event EventHandler<BssbServer?>? ServerSelectedEvent;
        public event EventHandler<BssbServer>? ConnectClickedEvent;

        #region Lifecycle

        [UIAction("#post-parse")]
        [UsedImplicitly]
        private void PostParse()
        {
            // Connect CustomListTag's ScrollView to the up/down buttons and scrollbar
            _scrollView = _serverList.gameObject.GetComponentInChildren<ScrollView>();
            _scrollView._verticalScrollIndicator = _scrollIndicator;
            _scrollView._pageDownButton = _pageDownButton;
            _scrollView._pageUpButton = _pageUpButton;
            _scrollView._verticalScrollIndicator.gameObject.SetActive(false); // hide initially because it bugs out
            _lastScrollPosApprox = _scrollView.position;

            if (!_bsmlReady)
            {
                _serverList.TableView.didReloadDataEvent += HandleTableViewDataReloaded;
                
                _bsmlReady = true;
            }

            // Attach loading control
            _loadingControl = BssbLoadingControl.Create(_serverList.transform);

            if (_loadingControl != null)
            {
                _loadingControl.didPressRefreshButtonEvent += HandleRefreshButtonClick;
                UpdateLoadingState();
            }

            // Make entire main view background raycast target; makes it easier to aim, feels nicer
            var dummyBg = _mainContentRoot.gameObject.GetComponent<ImageView>()
                          ?? _mainContentRoot.gameObject.AddComponent<ImageView>();
            dummyBg.color = Color.clear;
            dummyBg.raycastTarget = true;

            // Filter states
            RefreshFilterStates();

            // Trigger initial refresh
            if (!_browser.IsLoading)
                HandleRefreshButtonClick();
        }

        public async void OnEnable()
        {
            ResetSelection();

            _browser.UpdateEvent += HandleBrowserUpdate;
            _browser.QueryParams = _config.FilterSet ?? new();

            RefreshFilterStates();

            _browser.EnableDiscovery();
            await _browser.ResetRefresh();
        }

        public void OnDisable()
        {
            _browser.UpdateEvent -= HandleBrowserUpdate;
            
            _parserParams.EmitEvent("closeSearchKeyboard");

            _browser.CancelLoading();
            _browser.DisableDiscovery();
        }

        protected override void OnDestroy()
        {
            if (_loadingControl != null)
            {
                _loadingControl.didPressRefreshButtonEvent -= HandleRefreshButtonClick;
                Destroy(_loadingControl.gameObject);
                _loadingControl = null;
            }

            base.OnDestroy();
        }

        protected void Update()
        {
            if (!_bsmlReady)
                return;
            
            // Manual scroll event detection; need to refresh cell extensions on scroll
            // The refresh logic is not fast, so we need to avoid calling it too often

            _lastScrollPosExact = _scrollView!.position;
            var scrollPosDiff = Mathf.Abs(_lastScrollPosExact - _lastScrollPosApprox);
            if (scrollPosDiff > 1f)
            {
                _lastScrollPosApprox = _scrollView.position;
                _refreshPending = true;
            }
            
            if (_refreshPending && Time.realtimeSinceStartup - _lastRefreshTime >= .025f)
            {
                RefreshCellContentsAndSelection();
            }
        }

        public void Dispose()
        {
            _serverList.TableView.didReloadDataEvent -= HandleTableViewDataReloaded;
        }

        #endregion

        #region Data/list

        private void HandleBrowserUpdate(object sender, EventArgs e)
        {
            if (!_bsmlReady)
                return;
            
            // Sometimes the non-primary buttons become disabled if the server browser
            //  isn't opened until after level selection, so let's ensure they're active
            _refreshButton.gameObject.SetActive(true);
            _filterButton.gameObject.SetActive(true);
            _createButton.gameObject.SetActive(true);

            UpdateLoadingState();

            if (_browser.IsLoading)
                // Only proceed with list sync on load completion
                return;
            
            // Retain this as our "before change" position
            _restoreScrollPos = _lastScrollPosExact;
            
            // Fill data
            SyncServerListData(_browser.GetAllServersSorted());
            AfterCellsChanged();

            // Service alert
            if (!string.IsNullOrWhiteSpace(_browser.MessageOfTheDay))
            {
                _floatingAlert.DismissAllPending();
                _floatingAlert.PresentNotification(new BssbFloatingAlert.NotificationData
                (
                    Sprites.AnnouncePadded,
                    "Service Message",
                    _browser.MessageOfTheDay!,
                    BssbLevelBarClone.BackgroundStyle.SolidBlue,
                    true
                ));
            }
            else
            {
                _floatingAlert.DismissPinned();
            }
        }

        private void SyncServerListData(List<BssbServer> sortedServers)
        {
            _serverList.Data = new List<CustomListTableData.CustomCellInfo>(
                sortedServers.Select(s => new BssbServerCellInfo(s))
            );
        }

        private void AfterCellsChanged()
        {
            CancelCoverArtLoading();
            
            _serverList.TableView.selectionType = TableViewSelectionType.Single;
            _serverList.TableView.ReloadData(); // should cause visibleCells to be updated

            RefreshCellContentsAndSelection();
        }

        private void HandleTableViewDataReloaded(TableView tableView)
        {
            RefreshCellContentsAndSelection();

            if (_scrollView != null && _restoreScrollPos > 0f)
            {
                _scrollView.ScrollTo(_restoreScrollPos, false);
            }
        }

        public void RefreshCellContentsAndSelection(BssbServer? overrideSelection = null)
        {
            _refreshPending = false;
            _lastRefreshTime = Time.realtimeSinceStartup;
            
            if (overrideSelection != null)
                _selectedServer = overrideSelection;

            var restoredSelection = false;

            foreach (var cell in _serverList.TableView.visibleCells.ToList())
            {
                if (cell == null)
                    continue;

                if (RefreshCell(cell))
                    restoredSelection = true;
            }

            if (restoredSelection)
                _selectionValid = true;
            else
                ResetSelection(false);

            _connectButton.interactable = _selectionValid;
        }

        private bool RefreshCell(TableCell cell)
        {
            var extensions = cell.gameObject.GetComponent<BssbServerCellExtensions>();
            var cellInfo = (_serverList.Data[cell.idx] as BssbServerCellInfo)!;
            var restoredSelection = false;

            if (_selectedServer is not null && cellInfo.Server.Key == _selectedServer.Key)
            {
                _serverList.TableView.SelectCellWithIdx(cell.idx);
                restoredSelection = true;
            }

            if (extensions == null)
            {
                extensions = cell.gameObject.AddComponent<BssbServerCellExtensions>();
                _di.Inject(extensions);
            }

            extensions.SetData(cell, cellInfo, cellInfo.Server);
            _ = extensions.SetCoverArt(_coverArtCts!.Token);
            return restoredSelection;
        }

        private void UpdateLoadingState()
        {
            if (!_bsmlReady)
                return;

            var showLoading = _browser.IsLoading;
            var showError = !showLoading && _browser.ApiRequestFailed;
            var showNoData = !showLoading && !showError && _browser.AllServers.IsEmpty();

            if (_loadingControl != null)
            {
                if (showLoading)
                    _loadingControl.ShowLoading("Loading servers...");
                else if (showError)
                    _loadingControl.ShowText("Failed to load servers", true);
                else if (showNoData)
                    _loadingControl.ShowText(
                        _browser.QueryParams.AnyFiltersActive
                            ? "No servers found (filters active)"
                            : "No servers found", true);
                else
                    _loadingControl.Hide();
            }

            if (showLoading)
            {
                _refreshButton.interactable = false;
                _filterButton.interactable = false;
                _connectButton.interactable = false;
                _pageUpButton.interactable = false;
                _pageDownButton.interactable = false;
                _serverList.TableView.gameObject.SetActive(false);
                _paginatorText.gameObject.SetActive(false);

                _parserParams.EmitEvent("closeSearchKeyboard");
            }
            else
            {
                _refreshButton.interactable = true;
                _filterButton.interactable = true;
                _serverList.TableView.gameObject.SetActive(true);

                var showCount = _browser.AllServers.Count > 0;
                
                if (showCount)
                {
                    var lobbiesForm = _browser.AllServers.Count == 1 ? "lobby" : "lobbies";
                    _paginatorText.SetText($"Found {_browser.AllServers.Count} {lobbiesForm}");
                }

                _paginatorText.gameObject.SetActive(showCount);
            }
        }

        private void ResetSelection(bool hard = true)
        {
            if (_bsmlReady)
            {
                _serverList.TableView.ClearSelection();
                _connectButton.interactable = false;
            }

            if (hard)
            {
                _selectedServer = null;
                ServerSelectedEvent?.Invoke(this, null);
            }

            _selectionValid = false;
        }

        public void CancelCoverArtLoading()
        {
            _coverArtCts?.Cancel();
            _coverArtCts?.Dispose();

            _coverArtCts = new();
        }

        #endregion

        #region BSML Actions

        [UIAction("refreshButtonClick")]
        private async void HandleRefreshButtonClick()
        {
            RefreshStartedEvent?.Invoke(this, EventArgs.Empty);
            await _browser.Refresh();
        }

        [UIAction("createButtonClick")]
        [UsedImplicitly]
        private void HandleCreateButtonClick()
        {
            CreateServerClickedEvent?.Invoke(this, EventArgs.Empty);
        }

        [UIAction("connectButtonClick")]
        [UsedImplicitly]
        private void HandleConnectButtonClick()
        {
            if (_selectedServer is null || !_selectionValid)
                return;

            ConnectClickedEvent?.Invoke(this, _selectedServer);
        }

        [UIAction("listSelect")]
        [UsedImplicitly]
        private void ListSelect(TableView tableView, int row)
        {
            var selectedCell = _serverList.Data[row] as BssbServerCellInfo;

            if (selectedCell is null)
                return;
            
            _selectedServer = selectedCell.Server;

            if (_selectedServer == null)
            {
                ResetSelection();
                return;
            }

            _selectionValid = true;
            _connectButton.interactable = true;

            ServerSelectedEvent?.Invoke(this, _selectedServer);
        }
        
        [UIAction("pageUpButtonClick")]
        [UsedImplicitly]
        private void HandlePageUpButtonClick()
        {
            _scrollView?.PageUpButtonPressed();
        }

        [UIAction("pageDownButtonClick")]
        [UsedImplicitly]
        private void HandlePageDownButtonClick()
        {
            _scrollView?.PageDownButtonPressed();
        }

        #endregion

        #region Search / Filters

        [UIValue("searchValue")]
        public string SearchValue
        {
            get => _browser.QueryParams.TextSearch ?? "";
            set => _browser.QueryParams.TextSearch = value;
        }

        [UIAction("filterButtonClick")]
        [UsedImplicitly]
        private void HandleFilterButtonClick()
        {
            _parserParams.EmitEvent("openSearchKeyboard");
        }

        [UIAction("filterFullClick")]
        [UsedImplicitly]
        private void HandleFilterFullClick()
        {
            _browser.QueryParams.HideFullGames = !_browser.QueryParams.HideFullGames;
            RefreshFilterStates();
        }

        [UIAction("filterInProgressClick")]
        [UsedImplicitly]
        private void HandleFilterInProgressClick()
        {
            _browser.QueryParams.HideInProgressGames = !_browser.QueryParams.HideInProgressGames;
            RefreshFilterStates();
        }

        [UIAction("filterVanillaClick")]
        [UsedImplicitly]
        private void HandleFilterVanillaClick()
        {
            _browser.QueryParams.HideVanillaGames = !_browser.QueryParams.HideVanillaGames;
            RefreshFilterStates();
        }

        [UIAction("filterQuickPlayClick")]
        [UsedImplicitly]
        private void HandleFilterQuickPlayClick()
        {
            _browser.QueryParams.HideQuickPlay = !_browser.QueryParams.HideQuickPlay;
            RefreshFilterStates();
        }

        [UIAction("searchKeyboardSubmit")]
        [UsedImplicitly]
        private async void HandleSearchKeyboardSubmit(string text)
        {
            _browser.QueryParams.TextSearch = text;
            RefreshFilterStates();

            // Make main content visible again
            _mainContentRoot.gameObject.SetActive(true);

            // Go back to first page & refresh
            RefreshStartedEvent?.Invoke(this, EventArgs.Empty);
            await _browser.ResetRefresh();

            // Commit filter set to config
            _config.FilterSet = _browser.QueryParams;
        }

        private void RefreshFilterStates()
        {
            if (!_bsmlReady)
                return;

            _filterSubButtonFull.gameObject.SetActive(true);
            _filterSubButtonInProgress.gameObject.SetActive(true);
            _filterSubButtonVanilla.gameObject.SetActive(true);
            _filterSubButtonQuickPlay.gameObject.SetActive(true);

            _filterButton.SetButtonFaceAndUnderlineColor(_browser.QueryParams.AnyFiltersActive
                ? BssbColorScheme.Green
                : BssbColorScheme.White);
            _filterSubButtonFull.SetButtonFaceAndUnderlineColor(_browser.QueryParams.HideFullGames
                ? BssbColorScheme.Green
                : BssbColorScheme.White);
            _filterSubButtonInProgress.SetButtonFaceAndUnderlineColor(_browser.QueryParams.HideInProgressGames
                ? BssbColorScheme.Green
                : BssbColorScheme.White);
            _filterSubButtonVanilla.SetButtonFaceAndUnderlineColor(_browser.QueryParams.HideVanillaGames
                ? BssbColorScheme.Green
                : BssbColorScheme.White);
            _filterSubButtonQuickPlay.SetButtonFaceAndUnderlineColor(_browser.QueryParams.HideQuickPlay
                ? BssbColorScheme.Green
                : BssbColorScheme.White);
        }

        #endregion
    }
}