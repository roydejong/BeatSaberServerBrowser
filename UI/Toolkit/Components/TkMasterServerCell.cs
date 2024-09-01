using System;
using HMUI;
using JetBrains.Annotations;
using ServerBrowser.Assets;
using ServerBrowser.Data;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkMasterServerCell : LayoutComponent
    {
        public override GameObject GameObject => _rootGameObject;
        
        public const int CellSpacing = 1;
        public const int CellPadding = 2;

        private GameObject _rootGameObject = null!;
        private StackLayoutGroup _rootLayoutGroup = null!;
        private LayoutElement _rootLayoutElement = null!;
        private RectTransform _rootRectTransform = null!;
        private ImageView _contentBackground = null!;
        private bool _isSelected = false;

        private MasterServerRepository.MasterServerInfo? _masterServerInfo = null;
        
        private TkImageView _serverImage = null!;
        private TkText _serverNameText = null!;
        private TkText _subText = null!;

        public event Action<MasterServerRepository.MasterServerInfo>? ClickedEvent;

        public override void AddToContainer(LayoutContainer container)
        {
            // Root game object
            _rootGameObject = new GameObject("TkMasterServerCell")
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
            topContainer.PreferredHeight = 9f;

            BuildTopContentComponents(topContainer);
        }

        private void BuildTopContentComponents(LayoutContainer container)
        {
            // Image
            _serverImage = container.AddAvatarImage(9f, 9f);
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
            textContainer.PreferredHeight = 9f;

            // Primary title
            _serverNameText = new TkText();
            _serverNameText.AddToContainer(textContainer);
            _serverNameText.SetPreferredSize(100f, 4.5f);
            _serverNameText.SetText("Master server name");
            _serverNameText.SetFontSize(3.2f);

            // Secondary text
            _subText = new TkText();
            _subText.AddToContainer(textContainer);
            _subText.SetTextColor(BssbColors.InactiveGray);
            _subText.SetPreferredSize(100f, 4.25f);
            _subText.SetText("Master server description");
            _subText.SetFontSize(2.8f);
        }

        private void HandleSelectionStateChanged(NoTransitionsButton.SelectionState state) =>
            RefreshSelectionState(state);

        private void RefreshSelectionState(NoTransitionsButton.SelectionState? state = null)
        {
            if (_contentBackground == null)
                return;

            if (state == NoTransitionsButton.SelectionState.Pressed)
            {
                if (_masterServerInfo != null)
                {
                    TriggerButtonClickEffect();
                    ClickedEvent?.Invoke(_masterServerInfo);
                }

                return;
            }

            if (_isSelected)
            {
                _contentBackground.color = BssbColors.HighlightBlue;
                _contentBackground.color0 = BssbColors.White;
                _contentBackground.color1 = BssbColors.HoverGradientEnd;
                return;
            }

            if (state == NoTransitionsButton.SelectionState.Highlighted)
            {
                _contentBackground.color = BssbColors.ButtonBaseBgHover;
                _contentBackground.color0 = BssbColors.White;
                _contentBackground.color1 = BssbColors.HoverGradientEnd;
                return;
            }

            _contentBackground.color = BssbColors.ButtonBaseBg;
            _contentBackground.color0 = BssbColors.White;
            _contentBackground.color1 = BssbColors.White;
        }

        public void SetIsSelected(bool isSelected)
        {
            _isSelected = isSelected;
            
            if (_masterServerInfo != null)
                SetData(_masterServerInfo);

            RefreshSelectionState();
        }

        public void SetData(MasterServerRepository.MasterServerInfo masterServerInfo)
        {
            _masterServerInfo = masterServerInfo;
            
            _serverNameText.SetText(masterServerInfo.DisplayName);
            if (_masterServerInfo.Description != null)
                _subText.SetText(_masterServerInfo.Description);
            else
                _subText.SetText(_masterServerInfo.GraphUrlNoProtocol);
            
            if (_masterServerInfo.IsOfficial)
            {
                _ = _serverImage.SetBuiltinSprite(Sprites.OfficialServerIcon);
            }
            else
            {
                _ = string.IsNullOrWhiteSpace(masterServerInfo.ImageUrl)
                    ? _serverImage.SetPlaceholderSabers()
                    : _serverImage.SetRemoteImage(masterServerInfo.ImageUrl);
            }
        }
    }
}