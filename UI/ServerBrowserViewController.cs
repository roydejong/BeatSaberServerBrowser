using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using ServerBrowser.Core;
using ServerBrowser.UI.Items;
using ServerBrowser.Utils;
using SongCore.Utilities;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI
{
    public class ServerBrowserViewController : BeatSaberMarkupLanguage.ViewControllers.BSMLResourceViewController
    {
        public override string ResourceName => "ServerBrowser.UI.ServerBrowserViewController.bsml";

        #region Data/UI updates
        internal CancellationTokenSource _imageDownloadCancellation;
        private HostedGameData _selectedLobby = null;

        private void SetInitialUiState()
        {
            GameMp.FixLobbyBrowserTitle();

            StatusText.text = "Loading...";
            StatusText.color = Color.gray;

            PageUpButton.interactable = false;
            PageDownButton.interactable = false;
            RefreshButton.interactable = false;
            ConnectButton.interactable = false;

            ClearSelection();
            CancelImageDownloads();
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
                StatusText.text = "Sorry, no servers found";
                StatusText.color = Color.red;
            }
            else
            {
                StatusText.text = $"Found {HostedGameBrowser.TotalResultCount} "
                    + (HostedGameBrowser.TotalResultCount == 1 ? "server" : "servers")
                    + $" (Page {HostedGameBrowser.PageIndex + 1} of {HostedGameBrowser.TotalPageCount})";
                StatusText.color = Color.green;

                foreach (var lobby in HostedGameBrowser.LobbiesOnPage)
                {
                    GameList.data.Add(new HostedGameCell(_imageDownloadCancellation, CellUpdateCallback, lobby));
                }
            }

            GameList.tableView.ReloadData();
            GameList.tableView.selectionType = TableViewSelectionType.Single;

            ClearSelection();

            RefreshButton.interactable = true;
            PageUpButton.interactable = HostedGameBrowser.PageIndex > 0;
            PageDownButton.interactable = HostedGameBrowser.PageIndex < HostedGameBrowser.TotalPageCount - 1;
        }
        #endregion

        #region Lifecycle
        private static ServerBrowserViewController _instance;
        public static ServerBrowserViewController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = BeatSaberUI.CreateViewController<ServerBrowserViewController>();

                return _instance;
            }
        }

        public override void __Activate(bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.__Activate(addedToHierarchy, screenSystemEnabling);

            HostedGameBrowser.OnUpdate += LobbyBrowser_OnUpdate;

            SetInitialUiState();
            HostedGameBrowser.FullRefresh();
        }

        public override void __Deactivate(bool removedFromHierarchy, bool deactivateGameObject, bool screenSystemDisabling)
        {
            base.__Deactivate(removedFromHierarchy, deactivateGameObject, screenSystemDisabling);

            HostedGameBrowser.OnUpdate -= LobbyBrowser_OnUpdate;
            parserParams.EmitEvent("loadingModalClose");

            CancelImageDownloads(false);
        }
        #endregion

        #region UI Components
        [UIParams]
        public BeatSaberMarkupLanguage.Parser.BSMLParserParams parserParams;

        [UIComponent("statusText")]
        public TextMeshProUGUI StatusText;

        [UIComponent("lobbyList")]
        public CustomListTableData GameList;

        [UIComponent("createButton")]
        public Button CreateButton;

        [UIComponent("refreshButton")]
        public Button RefreshButton;

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
        [UIAction("refreshButtonClick")]
        internal void RefreshButtonClick()
        {
            SetInitialUiState();
            HostedGameBrowser.FullRefresh();
        }

        [UIAction("createButtonClick")]
        internal void CreateButtonClick()
        {
            GameMp.OpenCreateServerMenu();
        }

        [UIAction("connectButtonClick")]
        internal void ConnectButtonClick()
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
        internal void Select(TableView tableView, int row)
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
        internal void PageUpButtonClick()
        {
            if (HostedGameBrowser.PageIndex > 0)
            {
                CancelImageDownloads();
                HostedGameBrowser.LoadPage((HostedGameBrowser.PageIndex - 1) * HostedGameBrowser.PageSize);
            }
        }

        [UIAction("pageDownButtonClick")]
        internal void PageDownButtonClick()
        {
            if (HostedGameBrowser.PageIndex < HostedGameBrowser.TotalPageCount - 1)
            {
                CancelImageDownloads();
                HostedGameBrowser.LoadPage((HostedGameBrowser.PageIndex + 1) * HostedGameBrowser.PageSize);
            }
        }

        internal void CellUpdateCallback(HostedGameCell cell)
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