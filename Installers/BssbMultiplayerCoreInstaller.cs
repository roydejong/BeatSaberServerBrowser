using Zenject;

namespace ServerBrowser.Installers
{
    public class BssbMultiplayerCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            // TODO Create persistent game object for timed invocations, in multiplayer only
            //Container.Bind<>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}