using Zenject;

namespace ServerBrowser.Installers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InitInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PluginConfig>().FromInstance(Plugin.Config).AsSingle();
        }
    }
}