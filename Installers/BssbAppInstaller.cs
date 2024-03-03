using ServerBrowser.UI.Toolkit;
using Zenject;

namespace ServerBrowser.Installers
{
    public class BssbAppInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BssbConfig>().FromInstance(Plugin.Config).AsSingle();
            
            Container.Bind<LayoutBuilder>().AsTransient();
        }
    }
}