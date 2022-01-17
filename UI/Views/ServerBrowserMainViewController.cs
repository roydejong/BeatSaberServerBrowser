using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.UI.Components;
using SiraUtil.Logging;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Views
{
    [HotReload]
    public class ServerBrowserMainViewController : BSMLAutomaticViewController
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbBrowser _browser = null!;
        [Inject] private readonly CoverArtLoader _coverArtLoader = null!;

        [UIComponent("refreshButton")] private Button _refreshButton = null!;
        [UIComponent("filterButton")] private Button _filterButton = null!;
        [UIComponent("createButton")] private Button _createButton = null!;
        [UIComponent("connectButton")] private Button _connectButton = null!;
        [UIComponent("loadingRoot")] private HorizontalLayoutGroup _loadingRoot = null!;
        [UIComponent("scrollIndicator")] private VerticalScrollIndicator _scrollIndicator = null!;
        [UIComponent("pageUpButton")] private Button _pageUpButton = null!;
        [UIComponent("pageDownButton")] private Button _pageDownButton = null!;
        [UIComponent("serverList")] private CustomListTableData _gameList = null!;

        private bool _bsmlReady;
        private LoadingControl? _loadingControl;

        public event EventHandler<EventArgs>? CreateServerClickedEvent;

        #region Unity lifecycle

        [UIAction("#post-parse")]
        private void PostParse()
        {
            _bsmlReady = true;
            
            // Empty scroll indicator
            _scrollIndicator.normalizedPageHeight = 0f;
            _scrollIndicator.progress = 0f;
            
            // Attach loading control
            _loadingControl = BssbLoadingControl.Create(_loadingRoot.transform);

            if (_loadingControl != null)
            {
                _loadingControl.didPressRefreshButtonEvent += HandleRefreshButtonClick;
                UpdateLoadingState();
            }
            
            // Refresh
            HandleRefreshButtonClick();
        }

        public async void OnEnable()
        {
            UpdateLoadingState(true);

            _browser.UpdateEvent += HandleBrowserUpdate;
            await _browser.Reset();
        }

        public void OnDisable()
        {
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

            _log.Info($"!HandleBrowserUpdate!");
            
            // Clear out table view completely
            _gameList.data.Clear();
            _gameList.tableView.DeleteCells(0, _gameList.tableView.numberOfCells);
            
            // Sometimes the non-primary buttons become disabled if the server browser
            //  isn't opened until after level selection, so let's ensure they're active
            _refreshButton.gameObject.SetActive(true);
            _filterButton.gameObject.SetActive(true);
            _createButton.gameObject.SetActive(true);

            // Update loading state + scroll indicator
            UpdateLoadingState();
            
            // Fill data
            if (_browser.PageData?.Lobbies is not null)
            {
                _log.Warn("!Fill start!");
                foreach (var lobby in _browser.PageData.Lobbies)
                {
                    _log.Warn("!Fill item!");
                    _gameList.data.Add(new(lobby.Name, lobby.Key, Sprites.Crown));
                }
            }
                
            _gameList.tableView.RefreshCellsContent();
            _gameList.tableView.selectionType = TableViewSelectionType.Single;
            _gameList.tableView.ReloadData(); // should cause visibleCells to be updated
        }

        private void UpdateLoadingState(bool? setLoading = null)
        {
            if (!_bsmlReady)
                return;
            
            var showLoading = ((setLoading.HasValue && setLoading.Value) || _browser.IsLoading);
            var showError = _browser.LoadingErrored;
            var showNoData = !showLoading && !showError && (_browser.PageData?.Lobbies?.Count ?? 0) == 0;

            _log.Info(
                $"UpdateLoadingState (showLoading={showLoading}, showError={showError}, showNoData={showNoData})");

            if (_loadingControl != null)
            {
                if (showLoading)
                    _loadingControl.ShowLoading("Loading servers...");
                else if (showError)
                    _loadingControl.ShowText("Failed to load servers", true);
                else if (showNoData)
                    _loadingControl.ShowText("No servers found", true);
                else
                    _loadingControl.Hide();
            }

            if (showLoading)
            {
                _refreshButton.interactable = false;
                _connectButton.interactable = false;
                _pageUpButton.interactable = false;
                _pageDownButton.interactable = false;
                _gameList.tableView.gameObject.SetActive(false);
            }
            else
            {
                var pageCount = 1;
                var scrollProgress = 0f;
                var canMoveUp = false;
                var canMoveDown = false;

                if (_browser.PageData is not null)
                {
                    pageCount = _browser.PageData.PageCount;
                    scrollProgress = _browser.PageData.Offset / (float) _browser.PageData.Count;
                    canMoveUp = _browser.PageData.Offset > 0;
                    canMoveDown = (pageCount > 0 && scrollProgress < 1f);
                }

                _refreshButton.interactable = true;
                _pageUpButton.interactable = canMoveUp;
                _pageDownButton.interactable = canMoveDown;
                _scrollIndicator.normalizedPageHeight = (1f / pageCount);
                _scrollIndicator.progress = scrollProgress;
                
                _log.Error($"normalizedPageHeight={_scrollIndicator.normalizedPageHeight}, " +
                           $"progress={_scrollIndicator.progress}, pageCount={pageCount}");
                
                _gameList.tableView.gameObject.SetActive(true);
            }
        }

        #endregion

        #region BSML Actions

        [UIAction("refreshButtonClick")]
        private async void HandleRefreshButtonClick()
        {
            UpdateLoadingState(true);
            await _browser.Refresh();
        }

        [UIAction("createButtonClick")]
        private void HandleCreateButtonClick()
        {
            CreateServerClickedEvent?.Invoke(this, EventArgs.Empty);
        }

        [UIAction("pageUpButtonClick")]
        private void HandlePageUpButtonClick()
        {
            _log.Error("PageUp!");
            _ = _browser.PageUp();
        }

        [UIAction("pageDownButtonClick")]
        private void HandlePageDownButtonClick()
        {
            _log.Error("PageDown!");
            _ = _browser.PageDown();
        }

        #endregion
    }
}