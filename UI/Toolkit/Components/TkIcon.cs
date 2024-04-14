using System.Threading;
using System.Threading.Tasks;
using HMUI;
using JetBrains.Annotations;
using ServerBrowser.Assets;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkIcon : LayoutComponent
    {
        [Inject] private readonly MaterialAccessor _materialAccessor = null!;
        
        public override GameObject GameObject => _gameObject;

        protected GameObject? _gameObject;
        protected ImageView? _imageView;
        protected LayoutElement? _layoutElement;

        public virtual string GameObjectName => "TkIcon";

        public override void AddToContainer(LayoutContainer container)
        {
            _gameObject = new(GameObjectName)
            {
                layer = LayoutContainer.UiLayer
            };
            _gameObject.transform.SetParent(container.Transform, false);

            _imageView = _gameObject.AddComponent<ImageView>();
            _imageView.material = _materialAccessor.UINoGlowRoundEdge;

            _layoutElement = _gameObject.AddComponent<LayoutElement>();
        }

        public async Task SetSprite(string spriteName, CancellationToken cancellationToken)
        {
            if (_imageView == null)
                return;

            await _imageView.SetAssetSpriteAsync(spriteName);
        }

        public void SetPreferredSize(float? width, float? height)
        {
            if (_layoutElement == null)
                return;

            if (width.HasValue)
            {
                _layoutElement.preferredWidth = width.Value;
                _layoutElement.minWidth = width.Value;
            }

            if (height.HasValue)
            {
                _layoutElement.preferredHeight = height.Value;
                _layoutElement.minHeight = height.Value;
            }
        }
    }
}