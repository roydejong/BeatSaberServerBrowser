using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.UI.Toolkit;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public partial class MainBrowserViewController : ViewController, IInitializable
    {
        private void BuildLayout(LayoutContainer root)
        {
            root.InsertMargin(0f, 5f);

            var splitRoot = root.AddHorizontalLayoutGroup("BrowserSplit");
            splitRoot.PreferredHeight = 100f;

            BuildLeftLayout(splitRoot);
            splitRoot.InsertMargin(10f, 0f);
            BuildMainLayout(splitRoot);
        }

        private void BuildLeftLayout(LayoutContainer parent)
        {
            var leftContainer = parent.AddVerticalLayoutGroup("BrowserLeft", expandChildWidth: true,
                verticalFit: ContentSizeFitter.FitMode.PreferredSize, pivotPoint: new Vector2(0, 1f));
            leftContainer.PreferredWidth = 40f;

            leftContainer.AddButton("Quick Play", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.SaberClash, iconSize: 5f);
            leftContainer.AddButton("Create Server", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.Global, iconSize: 5f);
            leftContainer.AddButton("Join by Code", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.Lock, iconSize: 5f);
            
            leftContainer.InsertMargin(-1f, 3f);
            leftContainer.AddHorizontalLine(width: 35f);
            leftContainer.InsertMargin(-1f, 3f);
            
            leftContainer.AddButton("Edit Avatar", preferredWidth: 40f, preferredHeight: 13f,
                iconName: Sprites.Avatar, iconSize: 5f, clickAction: HandleEditAvatarClick);
        }

        private void BuildMainLayout(LayoutContainer parent)
        {
            var mainContainer = parent.AddVerticalLayoutGroup("BrowserMain",
                verticalFit: ContentSizeFitter.FitMode.PreferredSize,
                pivotPoint: new Vector2(0, 1f));
            mainContainer.PreferredWidth = 120f;

            var topBar = mainContainer.AddHorizontalLayoutGroup("TopBar");
            topBar.PreferredHeight = 10f;
        }
    }
}