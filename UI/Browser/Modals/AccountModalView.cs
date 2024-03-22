using ServerBrowser.Assets;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.UI.Toolkit.Modals;
using ServerBrowser.UI.Toolkit.Wrappers;
using ServerBrowser.Util;
using TMPro;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Browser.Modals
{
    public class AccountModalView : TkModalView
    {
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;

        public override float ModalWidth => 65f;
        public override float ModalHeight => 50f;

        private LayoutContainer _container = null!;
        private TkText _titleText = null!;
        private TkText _statusText = null!;
        private TkButton _button = null!;

        public override void Initialize()
        {
            var wrap = new LayoutContainer(_layoutBuilder, transform, false);

            _container = wrap.AddVerticalLayoutGroup("Content",
                padding: new RectOffset(4, 4, 4, 4),
                expandChildWidth: true, expandChildHeight: false);
            _container.SetBackground("round-rect-panel");

            _titleText = _container.AddText("Username", textAlignment: TextAlignmentOptions.Center);
            _statusText = _container.AddText("Status", textAlignment: TextAlignmentOptions.Center, fontSize: 2.8f);
            
            _container.InsertMargin(-1f, 4f);
            _container.AddHorizontalLine();
            
            _button = _container.AddButton("View profile in browser", iconName: Sprites.Spectator, iconSize: 3.2f);
        }

        public void SetData(UserInfo? userInfo, bool loggedIn)
        {
            if (userInfo == null)
            {
                _titleText.SetText("Not logged in");
                _titleText.SetTextColor(BssbColors.InactiveGray);

                _statusText.SetText("No local profile is available. Are you logged out or offline on Steam / Oculus?");
                _statusText.SetTextColor(BssbColors.White);
                _statusText.SetActive(true);

                _button.GameObject.SetActive(false);
                return;
            }

            _titleText.SetText(userInfo.userName);
            _titleText.SetTextColor(BssbColors.White);

            _statusText.SetText(loggedIn ? $"Logged in ({userInfo.platform.ToString()})" : "Logging in...");

            _button.GameObject.SetActive(true);
        }
    }
}