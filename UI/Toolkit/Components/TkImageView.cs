using System.Threading.Tasks;
using JetBrains.Annotations;
using ServerBrowser.Assets;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkImageView : TkIcon
    {
        private RemoteImageStore? _remoteImageStore;
        private string? _currentUrlValue;
        
        public override string GameObjectName => "TkImageView";

        public override void AddToContainer(LayoutContainer container)
        {
            base.AddToContainer(container);
            
            _remoteImageStore = container.Builder.RemoteImageStore;
        }

        public async Task SetBuiltinSprite(string sprite)
        {
            if (_imageView == null)
                return;

            await _imageView.SetAssetSpriteAsync(sprite);
            _currentUrlValue = null;
        }

        public async Task SetPlaceholderAvatar() =>
            await SetBuiltinSprite(Sprites.PlaceholderAvatar);

        public async Task SetPlaceholderSabers() =>
            await SetBuiltinSprite(Sprites.PlaceholderSabers);

        public async Task SetRemoteImage(string? url)
        {
            if (_imageView == null || _remoteImageStore == null)
                return;

            if (string.IsNullOrWhiteSpace(url))
                return;

            if (_currentUrlValue == url && _imageView.sprite != null)
                return;

            var sprite = await _remoteImageStore.LoadImageAsync(url!);

            if (sprite == null)
            {
                await SetPlaceholderAvatar();
                return;
            }

            _imageView.sprite = sprite;
            _currentUrlValue = url;
        }
    }
}