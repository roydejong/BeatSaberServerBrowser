using ServerBrowser.Integrators;
using ServerBrowser.UI.Browser;
using ServerBrowser.UI.Browser.Views;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Util;
using Zenject;

namespace ServerBrowser.Installers
{
    public class BssbMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<LayoutBuilder>().AsTransient();
            Container.Bind<MaterialAccessor>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<MainBrowserViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BrowserFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            
            Container.BindInterfacesAndSelfTo<MainMenuIntegrator>().AsSingle();
        }
    }
}