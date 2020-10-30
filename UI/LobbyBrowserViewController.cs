using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using LobbyBrowserMod.Core;
using LobbyBrowserMod.UI.Items;
using LobbyBrowserMod.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyBrowserMod.UI
{
    public class LobbyBrowserViewController : BeatSaberMarkupLanguage.ViewControllers.BSMLResourceViewController
    {
        public override string ResourceName => "LobbyBrowserMod.UI.LobbyBrowserViewController.bsml";

        #region Lifecycle
        private static LobbyBrowserViewController _instance;
        public static LobbyBrowserViewController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = BeatSaberUI.CreateViewController<LobbyBrowserViewController>();

                return _instance;
            }
        }

        public override void __Activate(bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.__Activate(addedToHierarchy, screenSystemEnabling);

            LobbyBrowser.OnUpdate += LobbyBrowser_OnUpdate;

            SetInitialUiState();
            SetLoading(true);
            LobbyBrowser.FullRefresh();
        }

        public override void __Deactivate(bool removedFromHierarchy, bool deactivateGameObject, bool screenSystemDisabling)
        {
            base.__Deactivate(removedFromHierarchy, deactivateGameObject, screenSystemDisabling);

            LobbyBrowser.OnUpdate -= LobbyBrowser_OnUpdate;
            parserParams.EmitEvent("loadingModalClose");
        }
        #endregion

        #region UI Components
        [UIParams]
        public BeatSaberMarkupLanguage.Parser.BSMLParserParams parserParams;

        [UIComponent("statusText")]
        public TextMeshProUGUI StatusText;

        [UIComponent("lobbyList")]
        public CustomCellListTableData LobbyList;

        [UIComponent("createButton")]
        public Button CreateButton;

        [UIComponent("refreshButton")]
        public Button RefreshButton;

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
            SetLoading(true);

            LobbyBrowser.FullRefresh();
        }

        [UIAction("createButtonClick")]
        internal void CreateButtonClick()
        {
            GameMp.OpenCreateServerMenu();
        }

        [UIAction("pageUpButtonClick")]
        internal void PageUpButtonClick()
        {
            if (LobbyBrowser.PageIndex > 0)
            {
                SetLoading(true);
                LobbyBrowser.LoadPage((LobbyBrowser.PageIndex - 1) * LobbyBrowser.PageSize);
            }
        }

        [UIAction("pageDownButtonClick")]
        internal void PageDownButtonClick()
        {
            if (LobbyBrowser.PageIndex < LobbyBrowser.TotalPageCount - 1)
            {
                SetLoading(true);
                LobbyBrowser.LoadPage((LobbyBrowser.PageIndex + 1) * LobbyBrowser.PageSize);
            }
        }
        #endregion

        #region Data/UI updates
        [UIValue("lobbyListContents")]
        public List<object> QueueItems = new List<object>(10);

        private void SetInitialUiState()
        {
            GameMp.FixLobbyBrowserTitle();

            StatusText.text = "Loading...";
            StatusText.color = Color.gray;

            PageUpButton.interactable = false;
            PageDownButton.interactable = false;
            RefreshButton.interactable = false;

            SetLoading(true);
        }

        private LoadingControl loadingSpinner;

        private void SetLoading(bool value)
        {
            if (value)
            {
                if (loadingSpinner == null)
                {
                    loadingSpinner = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<LoadingControl>().First(), LoadingModal.transform);
                }

                Destroy(loadingSpinner.GetComponent<Touchable>());

                parserParams.EmitEvent("loadingModalOpen");
                loadingSpinner.ShowLoading("Fetching servers...");
            }
            else
            {
                parserParams.EmitEvent("loadingModalClose");

                if (loadingSpinner != null)
                {
                    loadingSpinner.Hide();
                    Destroy(loadingSpinner);
                    loadingSpinner = null;
                }
            }
        }

        private void LobbyBrowser_OnUpdate()
        {
            QueueItems.Clear();

            if (!LobbyBrowser.ConnectionOk)
            {
                StatusText.text = "Failed to get server list";
                StatusText.color = Color.red;
            }
            else if (!LobbyBrowser.AnyResults)
            {
                StatusText.text = "Sorry, no servers found";
                StatusText.color = Color.red;
            }
            else
            {
                StatusText.text = $"Found {LobbyBrowser.TotalResultCount} "
                    + (LobbyBrowser.TotalResultCount == 1 ? "server" : "servers")
                    + $" (Page {LobbyBrowser.PageIndex + 1} of {LobbyBrowser.TotalPageCount})";
                StatusText.color = Color.green;

                foreach (var lobby in LobbyBrowser.LobbiesOnPage)
                {
                    QueueItems.Add(new LobbyUiItem(lobby));
                }
            }

            LobbyList?.tableView?.ReloadData();

            RefreshButton.interactable = true;
            PageUpButton.interactable = LobbyBrowser.PageIndex > 0;
            PageDownButton.interactable = LobbyBrowser.PageIndex < LobbyBrowser.TotalPageCount - 1;

            SetLoading(false);
        }
        #endregion
    }
}