using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.UI.Components;
using ServerBrowser.Utils;
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
            HostedGameBrowser.FullRefresh(SearchValue);
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
        private HostedGameData _selectedLobby = null;

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
            _selectedLobby = null;
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
            else if (!HostedGameBrowser.AnyResults)
            {
                if (IsSearching)
                {
                    StatusText.text = "No servers found matching your search";
                }
                else
                {
                    StatusText.text = "Sorry, no servers found";
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
                    GameList.data.Add(new HostedGameCell(_imageLoadCancellation, CellUpdateCallback, lobby));
                }
            }

            if (!MpSession.GetLocalPlayerHasMultiplayerExtensions())
            {
                StatusText.text += "\r\nMultiplayerExtensions not detected, hiding custom games";
                StatusText.color = Color.yellow;
            }

            GameList.tableView.ReloadData();
            GameList.tableView.selectionType = TableViewSelectionType.Single;

            ClearSelection();

            RefreshButton.interactable = true;

            SearchButton.interactable = (IsSearching || HostedGameBrowser.AnyResults);
            SearchButton.SetButtonText(IsSearching ? "<color=#ff0000>Search</color>" : "Search");

            PageUpButton.interactable = HostedGameBrowser.PageIndex > 0;
            PageDownButton.interactable = HostedGameBrowser.PageIndex < HostedGameBrowser.TotalPageCount - 1;
        }

        public bool IsSearching
        {
            get => !String.IsNullOrEmpty(SearchValue);
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
        #endregion

        #region UI Events
        private string _searchValue = "";

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
            HostedGameBrowser.FullRefresh(SearchValue);
        }

        [UIAction("searchButtonClick")]
        private void SearchButtonClick()
        {
            ClearSelection();
            parserParams.EmitEvent("openSearchKeyboard");
        }

        [UIAction("createButtonClick")]
        private void CreateButtonClick()
        {
            MpModeSelection.OpenCreateServerMenu();
        }

        [UIAction("connectButtonClick")]
        private void ConnectButtonClick()
        {
            if (_selectedLobby != null && !string.IsNullOrEmpty(_selectedLobby.ServerCode))
            {
                _selectedLobby.Join();
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

            var selectedLobbyItem = (HostedGameCell)selectedRow;
            _selectedLobby = selectedLobbyItem.Game;

            ConnectButton.interactable = true;
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

        private void CellUpdateCallback(HostedGameCell cell)
        {
            foreach (var visibleCell in GameList.tableView.visibleCells)
            {
                // This is some BSML witchcraft. BSML made our cell a clone of the game's LevelListTableCell, where
                //   the title component is _songNameText, which we use for finding the right cell to update.

                var frankenCell = visibleCell as LevelListTableCell;
                var titleTextComponent = frankenCell.GetField<TextMeshProUGUI>("_songNameText");

                if (titleTextComponent != null)
                {
                    if (titleTextComponent.text == cell.text)
                    {
                        GameList.tableView.RefreshCellsContent();
                        return;
                    }
                }
            }
        }
        #endregion
    }
}