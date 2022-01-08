using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using TMPro;
using UnityEngine.UI;

namespace ServerBrowser.UI
{
    [HotReload]
    public class ServerBrowserMainViewController : BSMLAutomaticViewController
    {
        #region BSML UI Components
        [UIParams]
        public BeatSaberMarkupLanguage.Parser.BSMLParserParams parserParams;

        [UIComponent("mainContentRoot")]
        public VerticalLayoutGroup MainContentRoot;

        [UIComponent("searchKeyboard")]
        public ModalKeyboard SearchKeyboard;

        [UIComponent("serverMessageText")]
        public TextMeshProUGUI ServerMessageText;

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

        #region BSML UI Bindings
        [UIValue("searchValue")] public string SearchValue;
        #endregion
    }
}