using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
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
    public class ServerBrowserMainViewController : BSMLAutomaticViewController
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

        public event EventHandler<EventArgs>? RefreshStartedEvent;
        public event EventHandler<EventArgs>? CreateServerClickedEvent;
        public event EventHandler<BssbServer?>? ServerSelectedEvent;
        public event EventHandler<BssbServer>? ConnectClickedEvent;

        #region Lifecycle

        [UIAction("#post-parse")]
        private void PostParse()
        {
            _bsmlReady = true;

            // Hide scroll bar initially
            DisableScrollBar(true);

            // Enlarge scroll up/down buttons
            _pageUpButton.GetComponentInChildren<ImageView>().rectTransform.sizeDelta = new Vector2(1.5f, 1.5f);
            _pageDownButton.GetComponentInChildren<ImageView>().rectTransform.sizeDelta = new Vector2(1.5f, 1.5f);

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
            
            // Trigger refresh
            if (!_browser.IsLoading)
                HandleRefreshButtonClick();
        }

        public async void OnEnable()
        {
            ResetSelection();

            _browser.UpdateEvent += HandleBrowserUpdate;
            _browser.QueryParams = _config.FilterSet ?? new(); 
                
            RefreshFilterStates();

            await _browser.ResetRefresh();
        }

        public void OnDisable()
        {
            _parserParams.EmitEvent("closeSearchKeyboard");
            
            _browser.CancelLoading();

            _browser.UpdateEvent -= HandleBrowserUpdate;
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

        #endregion

        #region Data/list

        private void HandleBrowserUpdate(object sender, EventArgs e)
        {
            if (!_bsmlReady)
                return;

            // Clear out table view completely
            _serverList.Data.Clear();
            _serverList.TableView.DeleteCells(0, _serverList.TableView.numberOfCells);

            // Sometimes the non-primary buttons become disabled if the server browser
            //  isn't opened until after level selection, so let's ensure they're active
            _refreshButton.gameObject.SetActive(true);
            _filterButton.gameObject.SetActive(true);
            _createButton.gameObject.SetActive(true);

            // Update loading state + scroll indicator
            UpdateLoadingState();

            // Fill data
            if (_browser.PageData?.Servers is not null)
            {
                foreach (var lobby in _browser.PageData.Servers)
                {
                    _serverList.Data.Add(new BssbServerCellInfo(lobby));
                }
            }

            AfterCellsCreated();
            
            // Service alert
            if (_browser.PageData is not null && !string.IsNullOrWhiteSpace(_browser.PageData.MessageOfTheDay))
            {
                _floatingAlert.DismissAllPending();
                _floatingAlert.PresentNotification(new BssbFloatingAlert.NotificationData
                (
                    Sprites.AnnouncePadded,
                    "Service Message",
                    _browser.PageData.MessageOfTheDay!,
                    BssbLevelBarClone.BackgroundStyle.SolidBlue,
                    true
                ));
            }
            else
            {
                _floatingAlert.DismissPinned();
            }
        }

        private void AfterCellsCreated()
        {
            CancelCoverArtLoading();
            
            _serverList.TableView.selectionType = TableViewSelectionType.Single;
            _serverList.TableView.ReloadData(); // should cause visibleCells to be updated

            TryRestoreSelection();
        }

        public void TryRestoreSelection(BssbServer? overrideSelection = null)
        {
            if (overrideSelection != null)
                _selectedServer = overrideSelection;
            
            var restoredSelection = false;

            foreach (var cell in _serverList.TableView.visibleCells)
            {
                var extensions = cell.gameObject.GetComponent<BssbServerCellExtensions>();
                var cellInfo = _serverList.Data[cell.idx] as BssbServerCellInfo;

                if (cellInfo is null)
                    continue;

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
            }

            if (restoredSelection)
                _selectionValid = true;
            else
                ResetSelection(false);
            
            _connectButton.interactable = _selectionValid;
        }
        
        private void UpdateLoadingState()
        {
            if (!_bsmlReady)
                return;

            var showLoading = _browser.IsLoading;
            var showError = !showLoading && _browser.LoadingErrored;
            var showNoData = !showLoading && !showError && (_browser.PageData?.Servers?.Count ?? 0) == 0;

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

                // Disable scroll bar, or hide it if we never initialized it with page data
                DisableScrollBar(hide: _browser.PageData is null);
            }
            else
            {
                if (_browser.PageData is not null)
                {
                    UpdateScrollBar(_browser.PageData.TotalResultCount > 0, true,
                        _browser.PageData.TotalResultCount, _browser.PageData.PageOffset,
                        _browser.PageData.PageSize);
                }
                
                _refreshButton.interactable = true;
                _filterButton.interactable = true;
                _serverList.TableView.gameObject.SetActive(true);
                
                if (_browser.PageData is not null && _browser.PageData.TotalResultCount > 0 &&
                    _browser.PageData.Servers is not null && !_browser.PageData.Servers.IsEmpty())
                {
                    _paginatorText.SetText($"Showing {_browser.PageData.LowerBoundNumber} - {_browser.PageData.UpperBoundNumber} of {_browser.PageData.TotalResultCount} servers (Page {_browser.PageData.PageNumber} of {_browser.PageData.PageCount})");
                    _paginatorText.gameObject.SetActive(true);
                }
                else
                {
                    _paginatorText.gameObject.SetActive(false);
                }
            }

            // Fix scrollbar (requires a delay)
            Task.Run(async () =>
            {
                await Task.Delay(100);
                _scrollIndicator.InvokeMethod<object, VerticalScrollIndicator>("RefreshHandle");
            });
        }

        private void ResetSelection(bool hard = true)
        {
            if (_bsmlReady)
            {
                _serverList.TableView.ClearSelection();
                _connectButton.interactable = false;
                
                // fix for CheckScrollInput (unity-beta nullrefs)
                var scrollView = _serverList.gameObject.GetComponentInChildren<ScrollView>();
                if (scrollView != null)
                    _di.Inject(scrollView);
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

        #region UI Helpers

        private void UpdateScrollBar(bool makeVisible, bool makeInteractable, int totalItems,
            int currentOffset, int pageSize)
        {
            if (makeVisible && makeInteractable)
            {
                var canScrollUp = currentOffset > 0;
                var pageUpperBound = currentOffset + pageSize;
                var canScrollDown = pageUpperBound < totalItems;

                _pageUpButton.interactable = canScrollUp;
                _pageDownButton.interactable = canScrollDown;

                if (totalItems > 0)
                {
                    // normalizedPageHeight = 0-1f range to control handle height, where 1f is the full scrollbar height
                    _scrollIndicator.normalizedPageHeight = ((float) pageSize / (float) totalItems);
                    
                    // progress = 0-1f range to set middle(?) of handle position
                    if (currentOffset <= 0)
                        _scrollIndicator.progress = 0f;
                    else if (pageUpperBound >= totalItems)
                        _scrollIndicator.progress = 1f;
                    else
                        _scrollIndicator.progress = ((float) currentOffset / (float) totalItems);

                    _scrollIndicator.SetField("_padding", .5f);
                }
            }
            else if (!makeInteractable)
            {
                _pageUpButton.interactable = false;
                _pageDownButton.interactable = false;
            }

            _pageUpButton.gameObject.SetActive(makeVisible);
            _pageDownButton.gameObject.SetActive(makeVisible);
            _scrollIndicator.gameObject.SetActive(makeVisible);
        }

        private void DisableScrollBar(bool hide) =>
            UpdateScrollBar(!hide, false, 0, 0, 0);

        #endregion

        #region BSML Actions

        [UIAction("refreshButtonClick")]
        private async void HandleRefreshButtonClick()
        {
            RefreshStartedEvent?.Invoke(this, EventArgs.Empty);
            await _browser.Refresh();
        }

        [UIAction("createButtonClick")]
        private void HandleCreateButtonClick()
        {
            CreateServerClickedEvent?.Invoke(this, EventArgs.Empty);
        }

        [UIAction("connectButtonClick")]
        private void HandleConnectButtonClick()
        {
            if (_selectedServer is null || !_selectionValid)
                return;

            ConnectClickedEvent?.Invoke(this, _selectedServer);
        }

        [UIAction("pageUpButtonClick")]
        private async void HandlePageUpButtonClick()
        {
            await _browser.PageUp();
        }

        [UIAction("pageDownButtonClick")]
        private async void HandlePageDownButtonClick()
        {
            await _browser.PageDown();
        }

        [UIAction("listSelect")]
        private void ListSelect(TableView tableView, int row)
        {
            _selectedServer = _browser.PageData?.Servers?[row];

            if (_selectedServer == null)
            {
                ResetSelection();
                return;
            }

            _selectionValid = true;
            _connectButton.interactable = true;

            ServerSelectedEvent?.Invoke(this, _selectedServer);
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
        private void HandleFilterButtonClick()
        {
            _parserParams.EmitEvent("openSearchKeyboard");
        }

        [UIAction("filterFullClick")]
        private void HandleFilterFullClick()
        {
            _browser.QueryParams.HideFullGames = !_browser.QueryParams.HideFullGames;
            RefreshFilterStates();
        }

        [UIAction("filterInProgressClick")]
        private void HandleFilterInProgressClick()
        {
            _browser.QueryParams.HideInProgressGames = !_browser.QueryParams.HideInProgressGames;
            RefreshFilterStates();
        }

        [UIAction("filterVanillaClick")]
        private void HandleFilterVanillaClick()
        {
            _browser.QueryParams.HideVanillaGames = !_browser.QueryParams.HideVanillaGames;
            RefreshFilterStates();
        }

        [UIAction("filterQuickPlayClick")]
        private void HandleFilterQuickPlayClick()
        {
            _browser.QueryParams.HideQuickPlay = !_browser.QueryParams.HideQuickPlay;
            RefreshFilterStates();
        }
        
        [UIAction("searchKeyboardSubmit")]
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
                ? BssbColorScheme.Green : BssbColorScheme.White);
            _filterSubButtonFull.SetButtonFaceAndUnderlineColor(_browser.QueryParams.HideFullGames
                ? BssbColorScheme.Green : BssbColorScheme.White);
            _filterSubButtonInProgress.SetButtonFaceAndUnderlineColor(_browser.QueryParams.HideInProgressGames
                ? BssbColorScheme.Green : BssbColorScheme.White);
            _filterSubButtonVanilla.SetButtonFaceAndUnderlineColor(_browser.QueryParams.HideVanillaGames
                ? BssbColorScheme.Green : BssbColorScheme.White);
            _filterSubButtonQuickPlay.SetButtonFaceAndUnderlineColor(_browser.QueryParams.HideQuickPlay
                ? BssbColorScheme.Green : BssbColorScheme.White);
        }

        #endregion
    }
}