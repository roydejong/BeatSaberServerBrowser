using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit.Wrappers
{
    public class TkButton
    {
        public readonly GameObject GameObject;

        private readonly NoTransitionsButton _button;
        private readonly TextMeshProUGUI _text;
        private readonly StackLayoutGroup _baseLayout;
        private readonly Transform _contentTransform;
        private readonly LayoutElement _layoutElement;
        private readonly HorizontalLayoutGroup _contentLayout;

        private ImageView? _icon;
        private Color? _highlightColor;

        public TkButton(GameObject gameObject)
        {
            GameObject = gameObject;

            _button = gameObject.GetComponent<NoTransitionsButton>();
            _text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _baseLayout = gameObject.GetComponent<StackLayoutGroup>();
            _contentTransform = gameObject.transform.Find("Content");
            _layoutElement = gameObject.GetOrAddComponent<LayoutElement>();

            // Add a sub horizontal layout group so we can have icons positioned to the left of the text
            var innerGameObject = new GameObject("ContentInner")
            {
                layer = LayoutContainer.UiLayer
            };
            innerGameObject.transform.SetParent(_contentTransform, false);
            var layoutGroup = innerGameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            _contentTransform = innerGameObject.transform;
            _text.transform.SetParent(_contentTransform, false);

            var contentSizeFitter = innerGameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Add hover effect
            _button.selectionStateDidChangeEvent += HandleButtonSelectionStateChange;
        }

        private void HandleButtonSelectionStateChange(NoTransitionsButton.SelectionState state)
        {
            // We have re-parented the text, which means we need to manually set the color on hover state
            _text.color =
                state is NoTransitionsButton.SelectionState.Highlighted or NoTransitionsButton.SelectionState.Pressed
                    ? (_highlightColor ?? BssbColors.White)
                    : BssbColors.InactiveGray;
            // And we can have the icon do the same! (unless we're doing a cool highlight thing)
            if (_icon != null && _highlightColor == null)
                _icon.color = _text.color;
        }

        public void SetText(string text)
            => _text.SetText(text);

        public void SetHighlightColor(Color color)
        {
            _highlightColor = color;
            if (_icon != null)
            {
                _icon.enabled = false;
                _icon.color = BssbColors.White;
                _icon.gradient = true;
                _icon._gradientDirection = ImageView.GradientDirection.Horizontal;
                _icon.color0 = BssbColors.BssbAccent;
                _icon.color1 = _highlightColor.Value;
                _icon.enabled = true;
            }
        }

        public void SetPadding(int padding)
            => _baseLayout.padding = new RectOffset(0, 0, 0, 0);

        public void SetPadding(int horizontal, int vertical)
            => _baseLayout.padding = new RectOffset(horizontal, horizontal, vertical, vertical);

        public void SetWidth(float width)
            => _layoutElement.preferredWidth = width;

        public void SetHeight(float height)
            => _layoutElement.preferredHeight = height;

        public void AddClickHandler(UnityAction action)
            => _button.onClick.AddListener(action);

        public void RemoveClickHandler(UnityAction action)
            => _button.onClick.RemoveListener(action);

        public void DisableSkew()
        {
            foreach (var image in _button.gameObject.GetComponentsInChildren<ImageView>())
            {
                image.enabled = false;
                image._skew = 0f;
                image.enabled = true;
            }
        }

        public async Task SetIconAsync(string spriteName, float width, float height)
        {
            if (_icon == null)
            {
                var spacer = LayoutContainer.CreateSpacer(_contentTransform, 1.5f, 0f);
                spacer.SetAsFirstSibling();

                var iconObject = new GameObject("Icon");
                iconObject.layer = LayoutContainer.UiLayer;
                iconObject.transform.SetParent(_contentTransform, false);
                iconObject.transform.SetAsFirstSibling();

                _icon = iconObject.AddComponent<ImageView>();
                _icon.material = Utilities.ImageResources.NoGlowMat;

                var iconLayout = iconObject.AddComponent<LayoutElement>();
                iconLayout.preferredWidth = width;
                iconLayout.preferredHeight = height;
            }

            await _icon.SetSpriteAsync(spriteName);
        }
    }
}