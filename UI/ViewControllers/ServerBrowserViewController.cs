using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using ServerBrowser.Core;
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

        #region Data/UI updates
        internal CancellationTokenSource _imageDownloadCancellation;
        private HostedGameData _selectedLobby = null;

        private void SetInitialUiState()
        {
            UpdateTitle();

            StatusText.text = "Loading...";
            StatusText.color = Color.gray;

            RefreshButton.interactable = false;
            SearchButton.interactable = false;
            ConnectButton.interactable = false;

            PageUpButton.interactable = false;
            PageDownButton.interactable = false;

            ClearSelection();
            CancelImageDownloads();
        }

        private static void UpdateTitle()
        {
            var mpFlowCoordinator = GameMp.ModeSelectionFlowCoordinator;

            mpFlowCoordinator.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("SetTitle", new object[] {
                "Server Browser", ViewController.AnimationType.In
            });
        }

        private void CancelImageDownloads(bool reset = true)
        {
            if (_imageDownloadCancellation != null)
            {
                _imageDownloadCancellation.Cancel();
                _imageDownloadCancellation.Dispose();
                _imageDownloadCancellation = null;
            }

            if (reset)
            {
                _imageDownloadCancellation = new CancellationTokenSource();
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
            CancelImageDownloads();

            if (!HostedGameBrowser.ConnectionOk)
            {
                StatusText.text = "Failed to get server list";
                StatusText.color = Color.red;
            }
            else if (!HostedGameBrowser.AnyResults)
            {
                if (!String.IsNullOrEmpty(SearchValue))
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

                if (!String.IsNullOrEmpty(SearchValue))
                {
                    StatusText.text += " (Filtered)";
                }

                foreach (var lobby in HostedGameBrowser.LobbiesOnPage)
                {
                    GameList.data.Add(new HostedGameCell(_imageDownloadCancellation, CellUpdateCallback, lobby));
                }
            }

            GameList.tableView.ReloadData();
            GameList.tableView.selectionType = TableViewSelectionType.Single;

            ClearSelection();

            RefreshButton.interactable = true;
            SearchButton.interactable = true;

            PageUpButton.interactable = HostedGameBrowser.PageIndex > 0;
            PageDownButton.interactable = HostedGameBrowser.PageIndex < HostedGameBrowser.TotalPageCount - 1;
        }
        #endregion

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

            CancelImageDownloads(false);
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
            GameMp.OpenCreateServerMenu();
        }

        [UIAction("connectButtonClick")]
        private void ConnectButtonClick()
        {
            if (_selectedLobby != null && !string.IsNullOrEmpty(_selectedLobby.ServerCode))
            {
                GameMp.ConnectToServerCode(_selectedLobby.ServerCode);
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
                CancelImageDownloads();
                HostedGameBrowser.LoadPage((HostedGameBrowser.PageIndex - 1) * HostedGameBrowser.PageSize, SearchValue);
            }
        }

        [UIAction("pageDownButtonClick")]
        private void PageDownButtonClick()
        {
            if (HostedGameBrowser.PageIndex < HostedGameBrowser.TotalPageCount - 1)
            {
                CancelImageDownloads();
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