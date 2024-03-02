using ServerBrowser.Integrators;
using ServerBrowser.UI.Browser;
using Zenject;

namespace ServerBrowser.Installers
{
    public class BssbMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BrowserFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            
            Container.BindInterfacesAndSelfTo<MainMenuIntegrator>().AsSingle();
        }
    }
}