using System;
using HMUI;
using JetBrains.Annotations;
using ServerBrowser.Assets;
using ServerBrowser.Data;
using ServerBrowser.UI.Browser.Views;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkServerCell : LayoutComponent
    {
        public const int CellSpacing = 1;
        public const int CellPadding = 2;

        private GameObject? _rootGameObject = null;
        private StackLayoutGroup? _rootLayoutGroup = null;
        private LayoutElement? _rootLayoutElement = null!;
        private RectTransform? _rootRectTransform = null;
        private ImageView? _contentBackground = null;

        private ServerRepository.ServerInfo? _serverInfo = null;
        private TkImageView? _serverImage = null;
        private TkText? _serverNameText = null;
        private TkText? _gameModeText = null;
        private TkText? _playerCountText = null;
        private TkText? _stateText = null;

        public event Action<ServerRepository.ServerInfo>? ClickedEvent;

        public override void AddToContainer(LayoutContainer container)
        {
            // Root game object
            _rootGameObject = new GameObject("TkServerCell")
            {
                layer = LayoutContainer.UiLayer
            };
            _rootGameObject.transform.SetParent(container.Transform, false);

            _rootLayoutElement = _rootGameObject.AddComponent<LayoutElement>();
            _rootRectTransform = _rootGameObject.transform as RectTransform;

            _rootLayoutGroup = _rootGameObject.AddComponent<StackLayoutGroup>();
            _rootLayoutGroup.padding = new RectOffset(CellSpacing, 0, 0, CellSpacing);
            _rootLayoutGroup.childForceExpandHeight = true;
            _rootLayoutGroup.childForceExpandWidth = true;
            _rootLayoutGroup.childAlignment = TextAnchor.MiddleCenter;

            // State toggle
            var toggle = _rootGameObject.AddComponent<NoTransitionsButton>();
            toggle.selectionStateDidChangeEvent += HandleSelectionStateChanged;

            // Content
            BuildContentComponents(container);
        }

        private void BuildContentComponents(LayoutContainer container)
        {
            // Content
            var content = new GameObject("Content")
            {
                layer = LayoutContainer.UiLayer
            };
            content.transform.SetParent(_rootGameObject!.transform, false);

            var contentLayoutGroup = content.AddComponent<VerticalLayoutGroup>();
            contentLayoutGroup.childForceExpandWidth = true;
            contentLayoutGroup.childForceExpandHeight = false;
            contentLayoutGroup.padding = new RectOffset(CellPadding, CellPadding, CellPadding, CellPadding);

            var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            var contentContainer = new LayoutContainer(container.Builder, content.transform, true);

            // Background
            _contentBackground = contentContainer.SetBackground("panel-top");
            _contentBackground.color = BssbColors.ButtonBaseBg;

            // Top/bottom
            var topContainer = contentContainer.AddHorizontalLayoutGroup("Top");
            topContainer.PreferredHeight = 9.75f;

            BuildTopContentComponents(topContainer);

            var bottomContainer = contentContainer.AddHorizontalLayoutGroup("Bottom", expandChildWidth: false,
                expandChildHeight: true, padding: new RectOffset(0, 0, 1, 0));
            bottomContainer.PreferredHeight = (MainBrowserViewController.CellHeight - 9.75f - (CellPadding * 2));

            BuildBottomContentComponents(bottomContainer);
        }

        private void BuildTopContentComponents(LayoutContainer container)
        {
            // Image
            _serverImage = container.AddAvatarImage(9.75f, 9.75f);
            _ = _serverImage.SetPlaceholderAvatar();

            // Content text vertical
            var textGameObject = new GameObject("Text")
            {
                layer = LayoutContainer.UiLayer
            };
            textGameObject.transform.SetParent(container.Transform, false);

            var textLayoutGroup = textGameObject.AddComponent<VerticalLayoutGroup>();
            textLayoutGroup.padding = new RectOffset(CellPadding, CellPadding, 0, 0);

            var textContainer = new LayoutContainer(container.Builder, textGameObject.transform, true);
            textContainer.PreferredHeight = 9.75f;

            // Server text
            _serverNameText = new TkText();
            _serverNameText.AddToContainer(textContainer);
            _serverNameText.SetPreferredSize(100f, 4.875f);
            _serverNameText.SetText("Server name");
            _serverNameText.SetFontSize(3.4f);

            // Game mode
            _gameModeText = new TkText();
            _gameModeText.AddToContainer(textContainer);
            _gameModeText.SetTextColor(BssbColors.InactiveGray);
            _gameModeText.SetPreferredSize(100f, 4.875f);
            _gameModeText.SetText("Game mode");
            _gameModeText.SetFontSize(3.2f);
        }

        private void BuildBottomContentComponents(LayoutContainer container)
        {
            // Player count - left
            var playerCountObject = new GameObject("PlayerCount")
            {
                layer = LayoutContainer.UiLayer
            };
            playerCountObject.transform.SetParent(container.Transform, false);
            
            var playerCountLayoutGroup = playerCountObject.AddComponent<HorizontalLayoutGroup>();
            playerCountLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
            playerCountLayoutGroup.childForceExpandWidth = false;
            playerCountLayoutGroup.childForceExpandHeight = false;
            playerCountLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            
            var playerCountContainer = new LayoutContainer(container.Builder, playerCountObject.transform);
            var playerCountLayoutElement = playerCountObject.GetComponent<LayoutElement>();
            playerCountLayoutElement.preferredWidth = 9.75f;
            playerCountLayoutElement.minWidth = 9.75f;
            playerCountLayoutElement.preferredHeight = 4.25f;
            playerCountLayoutElement.minHeight = 4.25f;
            
            playerCountContainer.SetBackground("panel-top");

            playerCountContainer.AddIcon(Sprites.Player, 4f, 4f);
            
            _playerCountText = playerCountContainer.AddText("?/?", width: 5.75f, height: 4.25f);
            _playerCountText.SetFontSize(2.9f);
            
            // Spacer
            container.InsertMargin(CellPadding, 4.25f);
            
            // State text
            _stateText = container.AddText("Game state", fontSize: 2.9f, width: 50f, height: 4.25f);
            _stateText.SetTextColor(BssbColors.InactiveGray);
        }

        private void HandleSelectionStateChanged(NoTransitionsButton.SelectionState state)
        {
            if (_contentBackground == null)
                return;

            switch (state)
            {
                case NoTransitionsButton.SelectionState.Pressed:
                    if (_serverInfo != null)
                    {
                        TriggerButtonClickEffect();
                        ClickedEvent?.Invoke(_serverInfo);
                    }
                    break;
                case NoTransitionsButton.SelectionState.Highlighted:
                    _contentBackground.color = BssbColors.ButtonBaseBgHover;
                    _contentBackground.color0 = BssbColors.White;
                    _contentBackground.color1 = BssbColors.HoverGradientEnd;
                    break;
                default:
                    _contentBackground.color = BssbColors.ButtonBaseBg;
                    _contentBackground.color0 = BssbColors.White;
                    _contentBackground.color1 = BssbColors.White;
                    break;
            }
        }

        public void SetSize(float width, float height)
        {
            if (_rootLayoutElement == null)
                return;

            _rootLayoutElement.preferredWidth = width;
            _rootLayoutElement.preferredHeight = height;

            if (_rootRectTransform == null)
                return;

            _rootRectTransform.sizeDelta = new Vector2(width, height);
        }

        public void SetPosition(float anchorX, float anchorY)
        {
            if (_rootRectTransform == null)
                return;

            _rootRectTransform.anchorMin = new Vector2(0f, 1f);
            _rootRectTransform.anchorMax = new Vector2(0f, 1f);
            _rootRectTransform.pivot = new Vector2(0f, 1f);
            _rootRectTransform.anchoredPosition = new Vector2(anchorX, anchorY);
        }

        public void SetData(ServerRepository.ServerInfo serverInfo)
        {
            _serverInfo = serverInfo;
            
            _serverNameText?.SetText(serverInfo.ServerName);
            _serverNameText?.SetTextColor(serverInfo.WasLocallyDiscovered ? BssbColors.HighlightBlue : BssbColors.White);
            
            _gameModeText?.SetText(serverInfo.GameModeName);
            
            _playerCountText?.SetText($"{serverInfo.PlayerCount}/{serverInfo.PlayerLimit}");
            _playerCountText?.SetTextColor(serverInfo.IsFull ? BssbColors.InactiveGray : BssbColors.White);

            var playerStateText = serverInfo.InGameplay ? "Playing level" : "In lobby";
            if (serverInfo.IsFull)
                playerStateText += " (Full)";
            var playerStateColor = (serverInfo.InGameplay || serverInfo.IsFull)
                ? BssbColors.HotPink
                : BssbColors.HighlightBlue;
            _stateText?.SetText(playerStateText);
            _stateText?.SetTextColor(playerStateColor);

            _ = string.IsNullOrWhiteSpace(serverInfo.ImageUrl)
                ? _serverImage?.SetPlaceholderSabers()
                : _serverImage?.SetRemoteImage(serverInfo.ImageUrl);
        }

        public void SetActive(bool active)
        {
            if (_rootGameObject == null)
                return;

            if (_rootGameObject.activeSelf == active)
                return;

            _rootGameObject.SetActive(active);
            HandleSelectionStateChanged(NoTransitionsButton.SelectionState.Normal);
        }
    }
}