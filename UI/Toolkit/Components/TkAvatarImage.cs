using System.Threading;
using System.Threading.Tasks;
using ServerBrowser.Assets;
using ServerBrowser.UI.Data;

namespace ServerBrowser.UI.Toolkit.Components
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TkAvatarImage : TkIcon
    {
        private AvatarStore? _avatarStore;
        
        public override string GameObjectName => "TkAvatarImage";

        public override void AddToContainer(LayoutContainer container)
        {
            base.AddToContainer(container);
            
            _avatarStore = container.Builder.AvatarStore;
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
    }
}