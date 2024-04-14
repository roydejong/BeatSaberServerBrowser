using JetBrains.Annotations;
using Zenject;

namespace ServerBrowser.UI.Forms
{
    [UsedImplicitly]
    public class QuickPlayFormExtender : FormExtender, IInitializable
    {
        [Inject] private readonly JoinQuickPlayViewController _joinQuickPlayViewController = null!;
        
        public void Initialize()
        {
            base.Initialize(_joinQuickPlayViewController);
        }
    }
}