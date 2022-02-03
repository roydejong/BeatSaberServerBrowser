using ServerBrowser.Core;
using ServerBrowser.UI;
using ServerBrowser.UI.Components;
using ServerBrowser.UI.Utils;
using ServerBrowser.UI.Views;
using Zenject;

namespace ServerBrowser.Installers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            // BSSB Core
            Container.BindInterfacesAndSelfTo<BssbBrowser>().AsSingle();
            Container.BindInterfacesAndSelfTo<BssbMenuDataCollector>().AsSingle();
            
            // UI Core
            Container.BindInterfacesAndSelfTo<ModeSelectionIntegrator>().AsSingle();
            Container.BindInterfacesAndSelfTo<CreateServerExtender>().AsSingle();
            Container.BindInterfacesAndSelfTo<CoverArtLoader>().FromNewComponentOnNewGameObject().AsSingle();

            // UI Views
            Container.Bind<ServerBrowserMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ServerBrowserDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ServerBrowserFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            
            // Helpers
            Container.BindInterfacesAndSelfTo<BssbFloatingAlertMenuInit>().AsSingle();
        }
    }
}