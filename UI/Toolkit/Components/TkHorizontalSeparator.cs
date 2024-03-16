using BeatSaberMarkupLanguage;
using HMUI;
using JetBrains.Annotations;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkHorizontalSeparator : LayoutComponent
    {
        private LayoutElement _layoutElement = null!;
        private ImageView _imageView = null!;

        private const float DefaultWidth = -1f;
        private const float DefaultThickness = .25f;

        public override void AddToContainer(LayoutContainer container)
        {
            var gameObject = new GameObject("TkHorizontalSeparator")
            {
                layer = LayoutContainer.UiLayer
            };
            gameObject.transform.SetParent(container.Transform, false);

            _layoutElement = gameObject.AddComponent<LayoutElement>();
            _layoutElement.flexibleHeight = 0;
            SetPreferredWidth(DefaultWidth);
            SetThickness(DefaultThickness);

            _imageView = gameObject.AddComponent<ImageView>();
            _imageView.sprite = Utilities.ImageResources.WhitePixel;
            _imageView.material = Utilities.ImageResources.NoGlowMat;
            _imageView.color = BssbColors.VeryLightGray;
        }

        public void SetThickness(float thickness)
        {
            _layoutElement.preferredHeight = thickness;
            _layoutElement.minHeight = thickness;
        }

        public void SetPreferredWidth(float width)
        {
            _layoutElement.preferredWidth = -1f;
        }

        public void SetGradient()
        {
            // Match style of HorizontalSeparator used in LeaderboardTableCells
            _imageView.color = Color.white;
            _imageView.color0 = BssbColors.VeryLightGray;
            _imageView.color1 = BssbColors.WhiteTransparent;
            _imageView.gradient = true;
        }
    }
}