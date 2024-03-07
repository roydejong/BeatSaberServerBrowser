using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HMUI;
using ServerBrowser.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit.Components
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TkAvatarImage : LayoutComponent
    {
        private GameObject? _gameObject;
        private ImageView? _imageView;
        private LayoutElement? _layoutElement;
        
        public override void AddToContainer(LayoutContainer container)
        {
            _gameObject = new("TkAvatarImage")
            {
                layer = LayoutContainer.UiLayer
            };
            _gameObject.transform.SetParent(container.Transform, false);
            
            _imageView = _gameObject.AddComponent<ImageView>();
            _imageView.material = Utilities.ImageResources.NoGlowMat;
            
            _layoutElement = _gameObject.AddComponent<LayoutElement>();
            
            _ = SetPlaceholderAvatar(null);
        }

        public async Task SetPlaceholderAvatar(CancellationToken? cancellationToken)
        {
            if (_imageView == null)
                return;

            await _imageView.SetSpriteAsync(Sprites.PlaceholderAvatar);
        }

        public void SetPreferredSize(float? width, float? height)
        {
            if (_layoutElement == null)
                return;

            if (width.HasValue)
                _layoutElement.preferredWidth = width.Value;
            if (height.HasValue)
                _layoutElement.preferredHeight = height.Value;
        }
    }
}