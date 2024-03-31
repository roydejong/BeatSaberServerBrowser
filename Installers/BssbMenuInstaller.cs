using ServerBrowser.UI;
using ServerBrowser.UI.Browser;
using ServerBrowser.UI.Browser.Views;
using ServerBrowser.UI.Toolkit;
using Zenject;

namespace ServerBrowser.Installers
{
    public class BssbMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<LayoutBuilder>().AsTransient();
            
            Container.Bind<MaterialAccessor>().AsSingle();
            Container.Bind<CloneHelper>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<BrowserFilterViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MainBrowserViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BrowserFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            
            Container.BindInterfacesAndSelfTo<MainMenuIntegrator>().AsSingle();
        }
    }
}