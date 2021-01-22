using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.UI.Components;
using SongCore.Utilities;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.ViewControllers
{
    public class ServerBrowserViewController : BeatSaberMarkupLanguage.ViewControllers.BSMLResourceViewController
    {
        public override string ResourceName => "ServerBrowser.UI.BSML.ServerBrowserViewController.bsml";


        #region Lifecycle
        public override void __Activate(bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.__Activate(addedToHierarchy, screenSystemEnabling);

            HostedGameBrowser.OnUpdate += LobbyBrowser_OnUpdate;

            SetInitialUiState();
            HostedGameBrowser.FullRefresh(SearchValue, FilterFull, FilterInProgress, FilterModded);
        }

        public override void __Deactivate(bool removedFromHierarchy, bool deactivateGameObject, bool screenSystemDisabling)
        {
            base.__Deactivate(removedFromHierarchy, deactivateGameObject, screenSystemDisabling);

            HostedGameBrowser.OnUpdate -= LobbyBrowser_OnUpdate;

            parserParams.EmitEvent("closeSearchKeyboard");

            CancelImageLoading(false);
        }
        #endregion

        #region Data/UI updates
        internal CancellationTokenSource _imageLoadCancellation;
        private HostedGameData _selectedGame = null;

        private void SetInitialUiState()
        {
            MpModeSelection.SetTitle("Server Browser");

            StatusText.text = "Loading...";
            StatusText.color = Color.gray;

            RefreshButton.interactable = false;
            SearchButton.interactable = false;
            ConnectButton.interactable = false;

            PageUpButton.interactable = false;
            PageDownButton.interactable = false;

            ClearSelection();
            CancelImageLoading();
        }

        private void CancelImageLoading(bool reset = true)
        {
            if (_imageLoadCancellation != null)
            {
                _imageLoadCancellation.Cancel();
                _imageLoadCancellation.Dispose();
                _imageLoadCancellation = null;
            }

            if (reset)
            {
                _imageLoadCancellation = new CancellationTokenSource();
            }
        }

        private void ClearSelection()
        {
            GameList?.tableView?.ClearSelection();
            ConnectButton.interactable = false;
            _selectedGame = null;
        }

        private void ClearSearch()
        {
            SearchValue = "";
        }

        private void LobbyBrowser_OnUpdate()
        {
            GameList.data.Clear();
            CancelImageLoading();

            if (!HostedGameBrowser.ConnectionOk)
            {
                StatusText.text = "Failed to get server list";
                StatusText.color = Color.red;
            }
            else if (!HostedGameBrowser.AnyResultsOnPage)
            {
                if (HostedGameBrowser.TotalResultCount == 0)
                {
                    if (IsSearching)
                    {
                        StatusText.text = "No servers found matching your search";
                    }
                    else
                    {
                        StatusText.text = "Sorry, no servers found";
                    }
                }
                else
                {
                    StatusText.text = "This page is empty";
                    RefreshButtonClick(); // this is awkward so force a refresh
                }

                StatusText.color = Color.red;
            }
            else
            {
                StatusText.text = $"Found {HostedGameBrowser.TotalResultCount} "
                    + (HostedGameBrowser.TotalResultCount == 1 ? "server" : "servers")
                    + $" (Page {HostedGameBrowser.PageIndex + 1} of {HostedGameBrowser.TotalPageCount})";

                StatusText.color = Color.green;

                if (IsSearching)
                {
                    StatusText.text += " (Filtered)";
                }

                foreach (var lobby in HostedGameBrowser.LobbiesOnPage)
                {
                    GameList.data.Add(new HostedGameCellData(_imageLoadCancellation, CellUpdateCallback, lobby));
                }
            }

            if (!MpSession.GetLocalPlayerHasMultiplayerExtensions())
            {
                StatusText.text += "\r\nMultiplayerExtensions not detected, hiding modded games";
                StatusText.color = Color.yellow;
                FilterModdedButton.interactable = false;
            }

            AfterCellsCreated();

            RefreshButton.interactable = true;

            SearchButton.interactable = (IsSearching || HostedGameBrowser.AnyResultsOnPage);
            SearchButton.SetButtonText(IsSearching ? "<color=#ff0000>Search</color>" : "Search");

            PageUpButton.interactable = HostedGameBrowser.PageIndex > 0;
            PageDownButton.interactable = HostedGameBrowser.PageIndex < HostedGameBrowser.TotalPageCount - 1;
        }

        public bool IsSearching
        {
            get => !String.IsNullOrEmpty(SearchValue) || FilterFull || FilterInProgress || FilterModded;
        }
        #endregion

        #region UI Components
        [UIParams]
        public BeatSaberMarkupLanguage.Parser.BSMLParserParams parserParams;

        [UIComponent("mainContentRoot")]
        public VerticalLayoutGroup MainContentRoot;

        [UIComponent("searchKeyboard")]
        public ModalKeyboard SearchKeyboard;

        [UIComponent("statusText")]
        public TextMeshProUGUI StatusText;

        [UIComponent("lobbyList")]
        public CustomListTableData GameList;

        [UIComponent("refreshButton")]
        public Button RefreshButton;

        [UIComponent("searchButton")]
        public Button SearchButton;

        [UIComponent("createButton")]
        public Button CreateButton;

        [UIComponent("connectButton")]
        public Button ConnectButton;

        [UIComponent("pageUpButton")]
        private Button PageUpButton;

        [UIComponent("pageDownButton")]
        private Button PageDownButton;

        [UIComponent("loadingModal")]
        public ModalView LoadingModal;

        [UIComponent("filterModded")]
        public Button FilterModdedButton;
        #endregion

        #region UI Events
        private string _searchValue = "";
        private bool _filterFull = false;
        private bool _filterInProgress = false;
        private bool _filterModded = false;

        [UIValue("searchValue")]
        public string SearchValue
        {
            get => _searchValue;
            set
            {
                _searchValue = value;
                NotifyPropertyChanged();
            }
        }

        public bool FilterFull
        {
            get => _filterFull;
            set
            {
                _filterFull = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(FilterFullColor));
            }
        }

        [UIValue("filterFullColor")]
        public string FilterFullColor
        {
            get
            {
                switch (FilterFull)
                {
                    case true:
                        return "#32CD32";
                    case false:
                        return "#FF0000";
                    default:
                        return "#FF0000";
                }
            }
        }

        public bool FilterInProgress
        {
            get => _filterInProgress;
            set
            {
                _filterInProgress = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(FilterInProgressColor));
            }
        }

        [UIValue("filterInProgressColor")]
        public string FilterInProgressColor
        {
            get
            {
                switch (FilterInProgress)
                {
                    case true:
                        return "#32CD32";
                    case false:
                        return "#FF0000";
                    default:
                        return "#FF0000";
                }
            }
        }

        public bool FilterModded
        {
            get => _filterModded;
            set
            {
                _filterModded = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(FilterModdedColor));
            }
        }

        [UIValue("filterModdedColor")]
        public string FilterModdedColor
        {
            get
            {
                switch (FilterModded)
                {
                    case true:
                        return "#32CD32";
                    case false:
                        return "#FF0000";
                    default:
                        return "#FF0000";
                }
            }
        }

        [UIAction("searchKeyboardSubmit")]
        private async void SearchKeyboardSubmit(string text)
        {
            Plugin.Log?.Debug($"Set server search query to: {text}");
            SearchValue = text;

            // Make main content visible again
            MainContentRoot.gameObject.SetActive(true);

            // Hit refresh
            RefreshButtonClick();
        }

        [UIAction("refreshButtonClick")]
        private void RefreshButtonClick()
        {
            SetInitialUiState();
            HostedGameBrowser.FullRefresh(SearchValue, FilterFull, FilterInProgress, FilterModded);
        }

        [UIAction("searchButtonClick")]
        private void SearchButtonClick()
        {
            ClearSelection();
            parserParams.EmitEvent("openSearchKeyboard");
        }

        [UIAction("filterfullClick")]
        private void FilterFullClick()
        {
            FilterFull = !FilterFull;
        }

        [UIAction("filterInProgressClick")]
        private void FilterInProgressClick()
        {
            FilterInProgress = !FilterInProgress;
        }

        [UIAction("filterModdedClick")]
        private void FilterModdedClick()
        {
            FilterModded = !FilterModded;
        }

        [UIAction("createButtonClick")]
        private void CreateButtonClick()
        {
            MpModeSelection.OpenCreateServerMenu();
        }

        [UIAction("connectButtonClick")]
        private void ConnectButtonClick()
        {
            if (_selectedGame != null && !string.IsNullOrEmpty(_selectedGame.ServerCode))
            {
                _selectedGame.Join();
            }
            else
            {
                ClearSelection();
            }   
        }

        [UIAction("listSelect")]
        private void ListSelect(TableView tableView, int row)
        {
            var selectedRow = GameList.data[row];

            if (selectedRow == null)
            {
                ClearSelection();
                return;
            }

            var cellData = (HostedGameCellData)selectedRow;
            _selectedGame = cellData.Game;

            ConnectButton.interactable = _selectedGame.canJoin;
        }

        [UIAction("pageUpButtonClick")]
        private void PageUpButtonClick()
        {
            if (HostedGameBrowser.PageIndex > 0)
            {
                CancelImageLoading();
                HostedGameBrowser.LoadPage((HostedGameBrowser.PageIndex - 1) * HostedGameBrowser.PageSize, SearchValue);
            }
        }

        [UIAction("pageDownButtonClick")]
        private void PageDownButtonClick()
        {
            if (HostedGameBrowser.PageIndex < HostedGameBrowser.TotalPageCount - 1)
            {
                CancelImageLoading();
                HostedGameBrowser.LoadPage((HostedGameBrowser.PageIndex + 1) * HostedGameBrowser.PageSize, SearchValue);
            }
        }

        private void CellUpdateCallback(HostedGameCellData cellInfo)
        {
            GameList.tableView.RefreshCellsContent();

            foreach (var cell in GameList.tableView.visibleCells)
            {
                var extensions = cell.gameObject.GetComponent<HostedGameCellExtensions>();

                if (extensions != null)
                {
                    extensions.RefreshContent((HostedGameCellData)GameList.data[cell.idx]);
                }
            }
        }
        #endregion

        #region UI Custom Behaviors
        private void AfterCellsCreated()
        {
            GameList.tableView.selectionType = TableViewSelectionType.Single;

            GameList.tableView.ReloadData(); // should cause visibleCells to be updated

            foreach (var cell in GameList.tableView.visibleCells)
            {
                var extensions = cell.gameObject.GetComponent<HostedGameCellExtensions>();
                var data = (HostedGameCellData)GameList.data[cell.idx];

                if (extensions == null)
                {
                    cell.gameObject.AddComponent<HostedGameCellExtensions>().Configure(cell, data);
                }
                else
                {
                    extensions.RefreshContent(data);
                }
            }

            ClearSelection();
        }
        #endregion
    }
}