using BS_Utils.Utilities;
using IPA;
using IPA.Config.Stores;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.UI;
using ServerBrowser.UI.Components;
using System.Net.Http;
using System.Reflection;
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
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var assemblyVersionStr = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

                var bsVersion = IPA.Utilities.UnityGame.GameVersion.ToString();

                return $"ServerBrowser/{assemblyVersionStr} (BeatSaber/{bsVersion}) ({PlatformId})";
            }
        }

        #region Lifecycle
        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, IPA.Config.Config config)
        {
            Instance = this;
            Log = logger;
            Config = config.Generated<PluginConfig>();

            // Modifiers tab (in-lobby) - register needs to happen really early for now
            // (https://github.com/monkeymanboy/BeatSaberMarkupLanguage/issues/67)
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

            // BS Events
            BSEvents.lateMenuSceneLoadedFresh += OnLateMenuSceneLoadedFresh;

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

            // Clean up events
            BSEvents.lateMenuSceneLoadedFresh -= OnLateMenuSceneLoadedFresh;
            MpSession.TearDown();

            // Try to cancel any host announcements we may have had
            GameStateManager.UnAnnounce();
        }
        #endregion

        #region Core events
        private void OnLateMenuSceneLoadedFresh(ScenesTransitionSetupDataSO obj)
        {
            // Bind multiplayer session events
            MpSession.SetUp();
            MpModeSelection.SetUp();

            // UI setup
            PluginUi.SetUp();

            // Initial state update
            GameStateManager.HandleUpdate();
        }
        #endregion

        #region Platform detection
        public const string PLATFORM_UNKNOWN = "unknown";
        public const string PLATFORM_STEAM = "steam";
        public const string PLATFORM_OCULUS = "oculus";

        public static string PlatformId { get; private set; } = PLATFORM_UNKNOWN;

        private async Task DetectPlatform()
        {
            PlatformId = PLATFORM_UNKNOWN;

            // Attempt to detect platform
            // --> NOTE: Currently this can hang for a long time (possibly until a level is finished), depending on which mods the user is using
            // --> This should be fixed in BS_Utils v1.6.2+
            var userInfo = await BS_Utils.Gameplay.GetUserInfo.GetUserAsync().ConfigureAwait(false);

            if (userInfo.platform == UserInfo.Platform.Oculus)
            {
                PlatformId = PLATFORM_OCULUS;
            }
            else
            {
                PlatformId = PLATFORM_STEAM;
            }

            Plugin.Log?.Debug($"Got platform user info: {userInfo.platform} / UID {userInfo.platformUserId}");
        }
        #endregion
    }
}
