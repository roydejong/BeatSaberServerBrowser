using IPA;
using IPA.Config.Stores;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.UI;
using ServerBrowser.UI.Components;
using System.Net.Http;
using System.Reflection;
using ServerBrowser.Game.Models;
using ServerBrowser.Presence;
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
        
        internal static PresenceManager PresenceManager { get; private set; }

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
        public void Init(IPALogger logger, IPA.Config.Config config)
        {
            Instance = this;
            Log = logger;
            Config = config.Generated<PluginConfig>();

            // Modifiers tab (in-lobby)
            LobbyConfigPanel.RegisterGameplayModifierTab();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            // Harmony
            Harmony = new HarmonyLib.Harmony(HarmonyId);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Assets
            Sprites.Initialize();
            
            // Bind events
            GameStateManager.SetUp();
            
            // Rich Presence
            PresenceManager = new PresenceManager();
            PresenceManager.Start(GameStateManager.Activity);

            // HTTP client
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", Plugin.UserAgent);
            HttpClient.DefaultRequestHeaders.Add("X-BSSB", "✔");
        }

        [OnExit]
        public async void OnApplicationQuit()
        {
            // Destroy update timer
            UpdateTimer.DestroyTimerObject();

            // Clean up events
            GameStateManager.TearDown();
            MpSession.TearDown();
            
            // Rich Presence
            PresenceManager = new PresenceManager();
            PresenceManager.Stop();

            // Try to cancel any host announcements we may have had
            await GameStateManager.UnAnnounce();
        }
        #endregion

        #region Core events
        private bool _gotFirstActivation = false;

        internal async void OnOnlineMenuActivated()
        {
            // Create/start update timer
            UpdateTimer.StartTimer();
            
            // Most things only need to be setup once
            if (_gotFirstActivation)
                return;
            _gotFirstActivation = true;

            Plugin.Log?.Info("Multiplayer / Online menu opened for the first time, setting up.");
            
            // Bind multiplayer session events
            MpSession.SetUp();
            MpModeSelection.SetUp();

            // UI setup
            PluginUi.SetUp();

            // Initial state update
            GameStateManager.HandleUpdate(false);

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
