using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using LobbyBrowserMod.Core;
using LobbyBrowserMod.UI.Items;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

            StatusText.text = "Loading...";
            StatusText.color = Color.gray;

            LobbyBrowser.OnUpdate += LobbyBrowser_OnUpdate;
            LobbyBrowser.FullRefresh();
        }

        public override void __Deactivate(bool removedFromHierarchy, bool deactivateGameObject, bool screenSystemDisabling)
        {
            base.__Deactivate(removedFromHierarchy, deactivateGameObject, screenSystemDisabling);

            LobbyBrowser.OnUpdate -= LobbyBrowser_OnUpdate;
        }
        #endregion

        #region Components
        [UIComponent("statusText")]
        public TextMeshProUGUI StatusText;

        [UIComponent("lobby-list")]
        public CustomCellListTableData LobbyList;
        #endregion

        #region Data
        [UIValue("lobby-options")]
        public List<object> QueueItems = new List<object>(10);

        private void LobbyBrowser_OnUpdate()
        {
            QueueItems.Clear();

            if (!LobbyBrowser.ConnectionOk)
            {
                StatusText.text = "Could not get lobbies from server";
                StatusText.color = Color.red;
            }
            else if (!LobbyBrowser.AnyResults)
            {
                StatusText.text = "Sorry, no games found.";
                StatusText.color = Color.red;
            }
            else
            {
                StatusText.text = $"Found {LobbyBrowser.TotalResultCount} servers";
                StatusText.color = Color.green;

                foreach (var lobby in LobbyBrowser.LobbiesOnPage)
                {
                    QueueItems.Add(new LobbyUiItem(lobby));
                }
            }

            LobbyList?.tableView?.ReloadData();
        }
        #endregion
    }
}