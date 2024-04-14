using ServerBrowser.Data;
using ServerBrowser.Models;
using ServerBrowser.Network.Discovery;
using ServerBrowser.UI.Toolkit;
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
            Container.BindInterfacesAndSelfTo<MasterServerRepository>().AsSingle();
            Container.BindInterfacesAndSelfTo<MultiplayerConfigManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ServerRepository>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<RemoteImageStore>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<DiscoveryClient>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}