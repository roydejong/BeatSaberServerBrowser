using ServerBrowser.Core;
using ServerBrowser.Network.Discovery;
using ServerBrowser.UI.Components;
using Zenject;

namespace ServerBrowser.Installers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<PluginConfig>().FromInstance(Plugin.Config).AsSingle();
            
            Container.BindInterfacesAndSelfTo<BssbApiClient>().AsSingle();
            Container.BindInterfacesAndSelfTo<BssbDataCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<BssbServerAnnouncer>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<BssbSessionNotifier>().AsSingle();
            Container.BindInterfacesAndSelfTo<ServerBrowserClient>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<DirectConnectionPatcher>().AsSingle();

            Container.BindInterfacesAndSelfTo<DiscoveryClient>().FromNewComponentOnNewGameObject().AsSingle();
            
            Container.BindInterfacesAndSelfTo<BssbFloatingAlert>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}