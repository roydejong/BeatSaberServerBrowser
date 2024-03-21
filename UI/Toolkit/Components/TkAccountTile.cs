using System;
using HMUI;
using JetBrains.Annotations;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkAccountTile : LayoutComponent
    {
        [Inject] private readonly MaterialAccessor _materialAccessor = null!;
        
        private GameObject _rootGameObject = null!;
        private TkImageView _selfAvatarImage = null!;
        private ImageView _contentBackground = null!;
        private Material _defaultBgMaterial = null!;
        private TkText _usernameText = null!;
        private TkText _subText = null!;
        
        public event Action? ClickedEvent;

        public override void AddToContainer(LayoutContainer container)
        {
            // Root game object
            _rootGameObject = new GameObject("TkAccountTile")
            {
                layer = LayoutContainer.UiLayer
            };
            _rootGameObject.transform.SetParent(container.Transform, false);
            
            var layoutGroup = _rootGameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childForceExpandHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.padding = new RectOffset(1, 0, 1, 1);

            var contentSizeFitter = _rootGameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            var layoutElement = _rootGameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 41f;
            
            // State toggle
            var toggle = _rootGameObject.AddComponent<NoTransitionsButton>();
            toggle.selectionStateDidChangeEvent += HandleSelectionStateChanged;
            
            // Wrap container, set bg
            var selfContainer = new LayoutContainer(container.Builder, _rootGameObject.transform);
            _contentBackground = selfContainer.SetBackground("round-rect-panel");
            _defaultBgMaterial = _contentBackground.material;
            
            // Avatar image (left side)
            _selfAvatarImage = selfContainer.AddAvatarImage(10f, 10f);
            _ = _selfAvatarImage.SetPlaceholderAvatar();
            
            // Text wrapper (right side)
            var textContainer = selfContainer.AddVerticalLayoutGroup("Text", expandChildWidth: true,
                verticalFit: ContentSizeFitter.FitMode.PreferredSize,
                padding: new RectOffset(2, 0, 0, 0));
            
            // Account name
            _usernameText = textContainer.AddText("Username", width: 27f, height: 5.5f, fontSize: 3.4f);
            _usernameText.SetTextColor(BssbColors.White);
            
            _subText = textContainer.AddText("Sub text", width: 27f, height: 4.5f, fontSize: 2.9f);
            _subText.SetTextColor(BssbColors.InactiveGray);
        }

        private void HandleSelectionStateChanged(NoTransitionsButton.SelectionState state)
        {
            if (_contentBackground == null)
                return;

            switch (state)
            {
                case NoTransitionsButton.SelectionState.Pressed:
                    TriggerButtonClickEffect();
                    ClickedEvent?.Invoke();
                    break;
                case NoTransitionsButton.SelectionState.Highlighted: 
                    _contentBackground.color = BssbColors.ButtonBaseBgHover;
                    _contentBackground.color0 = BssbColors.White;
                    _contentBackground.color1 = BssbColors.HoverGradientEnd;
                    _contentBackground.gradient = true;
                    _contentBackground.material = _materialAccessor.UINoGlowRoundEdge;
                    break;
                default:
                    _contentBackground.color = BssbColors.White;
                    _contentBackground.color0 = BssbColors.White;
                    _contentBackground.color1 = BssbColors.White;
                    _contentBackground.material = _defaultBgMaterial;
                    break;
            }
        }

        public void SetNoLocalUserInfo()
        {
            _ = _selfAvatarImage.SetPlaceholderAvatar();
            
            _usernameText.SetText("Logged out");
            _usernameText.SetTextColor(BssbColors.White);
            
            _subText.SetText("You are not logged in");
            _subText.SetTextColor(BssbColors.InactiveGray);
        }

        public void SetLoggingIn(string username)
        {
            _ = _selfAvatarImage.SetPlaceholderAvatar();
            
            _usernameText.SetText(username.StripTags());
            _usernameText.SetTextColor(BssbColors.White);
            
            _subText.SetText("Logging in...");
            _subText.SetTextColor(BssbColors.InactiveGray);
        }

        public void SetLoggedIn(string username, string? avatarUrl)
        {
            if (!string.IsNullOrEmpty(avatarUrl))
                _ = _selfAvatarImage.SetRemoteImage(avatarUrl);
            else
                _ = _selfAvatarImage.SetPlaceholderAvatar();
            
            _usernameText.SetText(username.StripTags());
            _usernameText.SetTextColor(BssbColors.White);
            
            _subText.SetText("Logged in");
            _subText.SetTextColor(BssbColors.HotPink);
        }

        public void SetLoginFailed(string username)
        {
            _ = _selfAvatarImage.SetPlaceholderAvatar();
            
            _usernameText.SetText(username.StripTags());
            _usernameText.SetTextColor(BssbColors.White);
            
            _subText.SetText("Login failed");
            _subText.SetTextColor(BssbColors.Orange);
        }
    }
}