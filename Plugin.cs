using IPA;
using IPA.Config;
using IPA.Config.Stores;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.UI.Components;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace ServerBrowser
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public const string HarmonyId = "mod.serverbrowser";

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static PluginConfig Config { get; private set; }

        internal static HarmonyLib.Harmony Harmony { get; private set; }
        internal static HttpClient HttpClient { get; private set; }

        public static string UserAgent
        {
            get
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var bsVersion = IPA.Utilities.UnityGame.GameVersion.ToString();

                return $"ServerBrowser/{assemblyVersion} (BeatSaber/{bsVersion})";
            }
        }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, Config conf)
        {
            Instance = this;
            Log = logger;
            Config = conf.Generated<PluginConfig>();

            // Modifiers tab (in-lobby)
            LobbyConfigPanel.RegisterGameplayModifierTab();
        }

        [OnStart]
        public async void OnApplicationStart()
        {
            Log?.Debug("OnApplicationStart");

            // Harmony
            Harmony = new HarmonyLib.Harmony(HarmonyId);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log?.Debug($"Harmony patching complete.");

            // Assets
            Sprites.Initialize();
            Log?.Debug($"Sprite conversion complete.");

            // HTTP client
            Log?.Info(UserAgent);

            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", Plugin.UserAgent);
            HttpClient.DefaultRequestHeaders.Add("X-BSSB", "✔");

            // Start update timer
            UpdateTimer.Start();

            // Detect platform
            // Note - currently (will be fixed in BS utils soon!): if the health warning is skipped (like in fpfc mode),
            //  this await will hang until a song is played, so the platform will be stuck on "unknown" til then
            await DetectPlatform();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log?.Debug("OnApplicationQuit");

            // Cancel update timer
            UpdateTimer.Stop();

            // Try to cancel any host announcements we may have had
            GameStateManager.UnAnnounce();
        }

        #region Platform detection
        public const string PLATFORM_UNKNOWN = "unknown";
        public const string PLATFORM_STEAM = "steam";
        public const string PLATFORM_OCULUS = "oculus";

        public static string PlatformId { get; private set; } = PLATFORM_UNKNOWN;

        private async Task DetectPlatform()
        {
            PlatformId = PLATFORM_UNKNOWN;
            
            var userInfo = await BS_Utils.Gameplay.GetUserInfo.GetUserAsync().ConfigureAwait(false);
            Plugin.Log.Info($"Got platform user info: {userInfo.platform} / UID {userInfo.platformUserId}");

            if (userInfo.platform == UserInfo.Platform.Oculus)
            {
                PlatformId = PLATFORM_OCULUS;
            }
            else
            {
                PlatformId = PLATFORM_STEAM;
            }
        }
        #endregion
    }
}
