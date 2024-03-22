using IPA;
using IPA.Config.Stores;
using JetBrains.Annotations;
using ServerBrowser.Installers;
using ServerBrowser.Models;
using SiraUtil.Web.SiraSync;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace ServerBrowser
{
    [Plugin(RuntimeOptions.DynamicInit)]
    [UsedImplicitly]
    public class Plugin
    {
        internal static IPALogger Log = null!;
        internal static BssbConfig Config = null!;

        [Init]
        public void Init(IPALogger logger, Zenjector zenjector, IPA.Config.Config config)
        {
            Log = logger;
            Config = config.Generated<BssbConfig>();

            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            zenjector.UseSiraSync(SiraSyncServiceType.GitHub, "roydejong", "BeatSaberServerBrowser");
            
            zenjector.Install<BssbAppInstaller>(Location.App);
            zenjector.Install<BssbMenuInstaller>(Location.Menu);
        }

        [OnEnable]
        public void OnEnable()
        {
        }

        [OnDisable]
        public void OnDisable()
        {
        }
    }
}