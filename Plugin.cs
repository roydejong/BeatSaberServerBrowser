using System.Reflection;
using IPA;
using IPA.Config.Stores;
using ServerBrowser.Assets;
using ServerBrowser.Installers;
using SiraUtil.Web.SiraSync;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace ServerBrowser
{
    [Plugin(RuntimeOptions.DynamicInit)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const string HarmonyId = "com.hippomade.serverbrowser";

        internal static IPALogger Log { get; private set; } = null!;
        internal static PluginConfig Config { get; private set; } = null!;

        private HarmonyLib.Harmony _harmony = null!;
        
        public static string UserAgent
        {
            get
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var assemblyVersionStr = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

                var bsVersion = IPA.Utilities.UnityGame.GameVersion.ToString();

                return $"ServerBrowser/{assemblyVersionStr} (BeatSaber/{bsVersion})";
            }
        }

        [Init]
        public void Init(IPALogger logger, IPA.Config.Config config, Zenjector zenjector)
        {
            Log = logger;
            Config = config.Generated<PluginConfig>();

            _harmony = new HarmonyLib.Harmony(HarmonyId);

            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            zenjector.UseSiraSync(SiraSyncServiceType.GitHub, "roydejong", "BeatSaberServerBrowser");
            
            zenjector.Install<AppInstaller>(Location.App);
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<MultiplayerCoreInstaller>(Location.MultiplayerCore);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            if (!Sprites.IsInitialized)
                Sprites.Initialize();
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}