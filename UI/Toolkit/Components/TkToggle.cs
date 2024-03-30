using JetBrains.Annotations;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkToggle : LayoutComponent
    {
        [Inject] private readonly SiraLog _logger = null!;
        [Inject] private readonly CloneHelper _cloneHelper = null!;

        public override void AddToContainer(LayoutContainer container)
        {
            var toggle = _cloneHelper.CloneTemplate(
                _cloneHelper.GetToggleTemplate(), container.Transform, "TkToggle"
            );
        }
    }
}