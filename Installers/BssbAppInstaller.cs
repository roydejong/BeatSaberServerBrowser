using ServerBrowser.Data;
using ServerBrowser.Session;
using ServerBrowser.UI.Data;
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
            Container.Bind<AvatarStore>().AsSingle();
        }
    }
}