using JetBrains.Annotations;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkDropdown : LayoutComponent
    {
        [Inject] private readonly SiraLog _logger = null!;
        [Inject] private readonly CloneHelper _cloneHelper = null!;

        public override void AddToContainer(LayoutContainer container)
        {
            var dropdown = _cloneHelper.CloneTemplate(
                _cloneHelper.GetDropdownTemplate(), container.Transform, "TkDropdown"
            );
            dropdown.SetTexts(new[] { "Option 1", "Option 2", "Option 3", "Option 4", "Option 5", "Option 6", "Option 7", "Option 8" });
        }
    }
}