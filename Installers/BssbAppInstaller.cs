using ServerBrowser.Core;
using Zenject;

namespace ServerBrowser.Installers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PluginConfig>().FromInstance(Plugin.Config).AsSingle();
            
            Container.BindInterfacesAndSelfTo<ServerBrowserClient>().AsSingle();
        }
    }
}