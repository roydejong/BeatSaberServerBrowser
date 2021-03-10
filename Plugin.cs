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

                return $"ServerBrowser/{assemblyVersionStr} (BeatSaber/{bsVersion}) ({MpLocalPlayer.PlatformId})";
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

            // Modifiers tab (in-lobby)
            LobbyConfigPanel.RegisterGameplayModifierTab();
        }

        [OnStart]
        public async void OnApplicationStart()
        {
            // Harmony
            Harmony = new HarmonyLib.Harmony(HarmonyId);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Assets
            Sprites.Initialize();

            // HTTP client
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", Plugin.UserAgent);
            HttpClient.DefaultRequestHeaders.Add("X-BSSB", "✔");

            // Start update timer
            UpdateTimer.Start();
        }

        [OnExit]
        public async void OnApplicationQuit()
        {
            Log?.Debug("OnApplicationQuit");

            // Cancel update timer
            UpdateTimer.Stop();

            // Clean up events
            MpSession.TearDown();

            // Try to cancel any host announcements we may have had
            await GameStateManager.UnAnnounce();
        }
        #endregion

        #region Core events
        private bool _gotFirstActivation = false;

        internal async void OnOnlineMenuActivated()
        {
            if (_gotFirstActivation)
            {
                return;
            }

            _gotFirstActivation = true;

            Plugin.Log?.Info("Multiplayer / Online menu opened for the first time, setting up.");

            // Bind multiplayer session events
            MpSession.SetUp();
            MpModeSelection.SetUp();

            // UI setup
            PluginUi.SetUp();

            // Initial state update
            GameStateManager.HandleUpdate();

            // Read local player info
            await MpLocalPlayer.SetUp();

            if (MpLocalPlayer.UserInfo != null)
            {
                // Update user-agent now the platform identifier can be added
                HttpClient.DefaultRequestHeaders.Remove("User-Agent");
                HttpClient.DefaultRequestHeaders.Add("User-Agent", Plugin.UserAgent);

                Log?.Info($"Running {UserAgent}");
            }
        }
        #endregion
    }
}
