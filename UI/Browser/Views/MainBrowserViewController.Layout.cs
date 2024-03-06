using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public partial class MainBrowserViewController : ViewController, IInitializable
    {
        private TkSearchInputField? _searchInputField;
        private TkFilterButton? _filterButton;
        private TkLoadingControl? _loadingControl;
        
        private void BuildLayout(LayoutContainer root)
        {
            root.InsertMargin(0f, 5f);

            var splitRoot = root.AddHorizontalLayoutGroup("BrowserSplit");
            splitRoot.PreferredHeight = 100f;

            BuildLeftLayout(splitRoot);
            splitRoot.InsertMargin(5f, 0f);
            BuildMainLayout(splitRoot);
        }

        private void BuildLeftLayout(LayoutContainer parent)
        {
            var leftContainer = parent.AddVerticalLayoutGroup("BrowserLeft", expandChildWidth: true,
                verticalFit: ContentSizeFitter.FitMode.PreferredSize, pivotPoint: new Vector2(0, 1f),
                padding: new RectOffset(0, 0, 1, 0));
            leftContainer.PreferredWidth = 42.5f;
            
            var pane = leftContainer.AddVerticalLayoutGroup("Pane", expandChildWidth: true,
                verticalFit: ContentSizeFitter.FitMode.PreferredSize, pivotPoint: new Vector2(0, 1f),
                padding: new RectOffset(0, 0, 1, 1));
            pane.SetBackground("panel-top");

            pane.AddButton("Quick Play", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.SaberClash, iconSize: 5f, noSkew: true, highlightColor: BssbColors.HighlightBlue,
                clickAction: HandleQuickPlayClick);
            pane.AddButton("Create Server", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.Global, iconSize: 5f, noSkew: true, highlightColor: BssbColors.HighlightBlue,
                clickAction: HandleCreateServerClick);
            pane.AddButton("Join by Code", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.Lock, iconSize: 5f, noSkew: true, highlightColor: BssbColors.HighlightBlue,
                clickAction: HandleJoinByCodeClick);
            
            pane.InsertMargin(-1f, 3f);
            pane.AddHorizontalLine(width: 35f);
            pane.InsertMargin(-1f, 3f);
            
            pane.AddButton("Edit Avatar", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.Avatar, iconSize: 5f, noSkew: true,
                clickAction: HandleEditAvatarClick);
        }

        private void BuildMainLayout(LayoutContainer parent)
        {
            var mainContainer = parent.AddVerticalLayoutGroup("BrowserMain",
                verticalFit: ContentSizeFitter.FitMode.PreferredSize,
                pivotPoint: new Vector2(0, 1f), childAlignment: TextAnchor.UpperLeft);
            mainContainer.PreferredWidth = 122.5f;

            var topBar = mainContainer.AddHorizontalLayoutGroup("TopBar");
            topBar.PreferredHeight = 10f;
            
            _searchInputField = topBar.AddSearchInputField("Search lobbies");
            _searchInputField.ChangedEvent += HandleSearchInputChanged;
            
            _filterButton = topBar.AddFilterButton("No Filters");
            _filterButton.ClickedEvent += HandleFilterButtonClicked;
            _filterButton.ClearedEvent += HandleFilterButtonCleared;
            
            var content = mainContainer.AddHorizontalLayoutGroup("Content", expandChildWidth: true,
                verticalFit: ContentSizeFitter.FitMode.PreferredSize, pivotPoint: new Vector2(0, 1f),
                padding: new RectOffset(0, 0, 1, 1));
            content.PreferredHeight = 65f;
            content.SetBackground("panel-top");
            
            _loadingControl = content.AddLoadingControl();
            _loadingControl.ShowLoading("Loading Servers");
        }
    }
}