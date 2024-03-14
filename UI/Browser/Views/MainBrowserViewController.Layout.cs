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
        private TkImageView? _selfAvatarImage;
        private TkText? _selfUsernameText;
        private TkScrollView? _scrollView;
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
            
            var userPane = leftContainer.AddHorizontalLayoutGroup("UserPane", expandChildHeight: true,
                horizontalFit: ContentSizeFitter.FitMode.PreferredSize,
                padding: new RectOffset(1, 0, 1, 1));
            userPane.SetBackground("round-rect-panel");
            userPane.PreferredWidth = 41f;
            
            _selfAvatarImage = userPane.AddAvatarImage(10f, 10f);
            _ = _selfAvatarImage.SetPlaceholderAvatar();
            userPane.InsertMargin(2f, -1f);
            _selfUsernameText = userPane.AddText("Username", width: 41f - 10f - 2f - 1f - 1f, height: 10f);
            
            leftContainer.InsertMargin(-1f, 2f);
            
            var pane = leftContainer.AddVerticalLayoutGroup("Pane", expandChildWidth: true,
                verticalFit: ContentSizeFitter.FitMode.PreferredSize, pivotPoint: new Vector2(0, 1f),
                padding: new RectOffset(0, 0, 3, 3));
            pane.SetBackground("panel-top");

            pane.AddButton("Quick Play", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.SaberClash, iconSize: 5f, noSkew: true, highlightColor: BssbColors.HighlightBlue,
                clickAction: HandleQuickPlayClicked);
            pane.AddButton("Create Server", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.Global, iconSize: 5f, noSkew: true, highlightColor: BssbColors.HighlightBlue,
                clickAction: HandleCreateServerClicked);
            pane.AddButton("Join by Code", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.Lock, iconSize: 5f, noSkew: true, highlightColor: BssbColors.HighlightBlue,
                clickAction: HandleJoinByCodeClicked);
            
            pane.InsertMargin(-1f, 1.5f);
            pane.AddHorizontalLine(width: 35f);
            pane.InsertMargin(-1f, 1.5f);
            
            pane.AddButton("Edit Avatar", preferredWidth: 40f, preferredHeight: 12f,
                iconName: Sprites.Avatar, iconSize: 5f, noSkew: true,
                clickAction: HandleEditAvatarClicked);
        }

        private void BuildMainLayout(LayoutContainer parent)
        {
            var mainContainer = parent.AddVerticalLayoutGroup("BrowserMain",
                verticalFit: ContentSizeFitter.FitMode.PreferredSize,
                pivotPoint: new Vector2(0, 1f), childAlignment: TextAnchor.UpperLeft);
            mainContainer.PreferredWidth = 122.5f;

            var topBar = mainContainer.AddHorizontalLayoutGroup("TopBar");
            topBar.PreferredHeight = 10f;
            
            _searchInputField = topBar.AddSearchInputField("Search Lobbies");
            _searchInputField.ChangedEvent += HandleSearchInputChanged;
            
            _filterButton = topBar.AddFilterButton("No Filters");
            _filterButton.ClickedEvent += HandleFilterButtonClicked;
            _filterButton.ClearedEvent += HandleFilterButtonCleared;
            
            var content = mainContainer.AddHorizontalLayoutGroup("Content", expandChildWidth: true,
                verticalFit: ContentSizeFitter.FitMode.PreferredSize, pivotPoint: new Vector2(0, 1f),
                padding: new RectOffset(0, 0, 1, 1));
            content.PreferredHeight = 69f;

            _scrollView = content.AddScrollView();
            _scrollView.SetScrollPerCellHeight(CellHeight);
            var svContent = _scrollView.Content!;
            
            _loadingControl = svContent.AddLoadingControl(57f);
            _loadingControl.RefreshClickedEvent += HandleRefreshClicked;
            _loadingControl.Hide();
        }
    }
}