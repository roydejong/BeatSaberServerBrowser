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

            var serverBrowserMainView = BeatSaberUI.CreateViewController<ServerBrowserMainViewController>();
            Container.Bind<ServerBrowserMainViewController>().FromInstance(serverBrowserMainView).AsSingle();
            
            var flowCoordinator = BeatSaberUI.CreateFlowCoordinator<ServerBrowserFlowCoordinator>();
            Container.Inject(flowCoordinator);
            Container.Bind<ServerBrowserFlowCoordinator>().FromInstance(flowCoordinator).AsSingle();
        }
    }
}