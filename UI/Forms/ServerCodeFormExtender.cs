using JetBrains.Annotations;
using Zenject;

namespace ServerBrowser.UI.Forms
{
    [UsedImplicitly]
    public class ServerCodeFormExtender : FormExtender, IInitializable
    {
        [Inject] private readonly ServerCodeEntryViewController _serverCodeEntryViewController = null!;
        
        public void Initialize()
        {
            base.Initialize(_serverCodeEntryViewController);
        }
    }
}