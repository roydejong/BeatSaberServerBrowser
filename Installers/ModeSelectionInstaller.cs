using BeatSaberMarkupLanguage;
using ServerBrowser.UI;
using Zenject;

namespace ServerBrowser.Installers
{
    public class ModeSelectionInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ModeSelectionController>().AsSingle();

            Container.Bind<ServerBrowserMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ServerBrowserFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}