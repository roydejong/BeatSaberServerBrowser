using ServerBrowser.UI;
using Zenject;

namespace ServerBrowser.Installers
{
    public class ModeSelectionInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ModeSelectionController>().AsSingle();
        }
    }
}