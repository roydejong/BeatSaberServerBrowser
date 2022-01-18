using ServerBrowser.Core;
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
            Container.BindInterfacesAndSelfTo<ServerBrowserClient>().AsSingle();
        }
    }
}