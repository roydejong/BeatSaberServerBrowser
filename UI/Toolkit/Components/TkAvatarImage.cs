using System.Threading;
using System.Threading.Tasks;
using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.UI.Data;
using ServerBrowser.UI.Util;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Toolkit.Components
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TkAvatarImage : LayoutComponent
    {
        [Inject] private readonly MaterialAccessor _materialAccessor = null!;
        
        private AvatarStore? _avatarStore;
        private GameObject? _gameObject;
        private ImageView? _imageView;
        private LayoutElement? _layoutElement;
        
        public override void AddToContainer(LayoutContainer container)
        {
            _avatarStore = container.Builder.AvatarStore;
            
            _gameObject = new("TkAvatarImage")
            {
                layer = LayoutContainer.UiLayer
            };
            _gameObject.transform.SetParent(container.Transform, false);
            
            _imageView = _gameObject.AddComponent<ImageView>();
            _imageView.material = _materialAccessor.UINoGlowRoundEdge;
            
            _layoutElement = _gameObject.AddComponent<LayoutElement>();
            
            _ = SetPlaceholderAvatar(CancellationToken.None);
        }

        public async Task SetPlaceholderAvatar(CancellationToken cancellationToken)
        {
            if (_imageView == null)
                return;

            await _imageView.SetAssetSpriteAsync(Sprites.PlaceholderAvatar);
        }
        
        public async Task SetAvatarFromUrl(string? url, CancellationToken cancellationToken)
        {
            if (_imageView == null || _avatarStore == null)
                return;

            if (string.IsNullOrWhiteSpace(url))
            {
                await SetPlaceholderAvatar(cancellationToken);
                return;
            }
            
            var sprite = await _avatarStore.LoadAvatarAsync(url!);
            
            if (sprite == null)
            {
                await SetPlaceholderAvatar(cancellationToken);
                return;
            }
            
            _imageView.sprite = sprite;
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