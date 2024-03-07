using ServerBrowser.Data;
using ServerBrowser.Session;
using Zenject;

namespace ServerBrowser.Installers
{
    public class BssbAppInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BssbConfig>().FromInstance(Plugin.Config).AsSingle();
            
            Container.BindInterfacesAndSelfTo<BssbApi>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<BssbSession>().AsSingle();
        }
    }
}