using ServerBrowser.UI;
using Zenject;

namespace ServerBrowser.Installers
{
    public class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ModeSelectionIntegrator>().AsSingle();

            Container.Bind<ServerBrowserMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ServerBrowserFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}