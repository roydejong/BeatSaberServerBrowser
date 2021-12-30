using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using IPA;
using IPA.Config.Stores;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.Game.Models;
using ServerBrowser.Presence;
using ServerBrowser.UI;
using ServerBrowser.UI.Components;
using IPALogger = IPA.Logging.Logger;

namespace ServerBrowser
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public const string HarmonyId = "mod.serverbrowser";

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static PluginConfig Config { get; private set; }
        internal static HarmonyLib.Harmony Harmony { get; private set; }
        
        internal static HttpClient HttpClient { get; private set; }

        internal static PresenceManager? PresenceManager { get; private set; }

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
            Harmony = new HarmonyLib.Harmony(HarmonyId);
        }

        [OnEnable]
        public void OnEnable()
        {
            _gotFirstActivation = false;
            
            // Harmony
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            // Modifiers tab (in-lobby)
            LobbyConfigPanel.RegisterGameplayModifierTab();

            // Assets
            if (!Sprites.IsInitialized)
                Sprites.Initialize();
            
            // Bind events
            MpEvents.OnlineMenuOpened += OnOnlineMenuOpened;
            MpEvents.OnlineMenuClosed += OnOnlineMenuClosed;
            
            // Core components
            GameStateManager.SetUp();

            // HTTP client
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", Plugin.UserAgent);
            HttpClient.DefaultRequestHeaders.Add("X-BSSB", "✔");
            
            // Launch arg for join (for Steam Rich presence etc)
            GlobalModState.AutoJoinBssbKey = LaunchArg.TryGetBssbKeyFromEnv();
            
            if (GlobalModState.AutoJoinBssbKey is not null)
                Log?.Info($"Game started with BSSB key launch arg: {GlobalModState.AutoJoinBssbKey}");
        }

        [OnDisable]
        public async void OnDisable()
        {
            _gotFirstActivation = false;
            
            // Unpatch Harmony
            Harmony.UnpatchSelf();
            
            // Modifiers tab (in-lobby)
            LobbyConfigPanel.RemoveGameplayModifierTab();
            
            // Destroy update timer
            UpdateTimer.DestroyTimerObject();

            // Clean up events
            MpEvents.OnlineMenuOpened -= OnOnlineMenuOpened;
            MpEvents.OnlineMenuClosed -= OnOnlineMenuClosed;
            
            // Core components 
            GameStateManager.TearDown();
            
            // Late init components
            MpSession.TearDown();
            MpModeSelection.TearDown();
            PluginUi.TearDown();
            
            // Rich Presence
            PresenceManager?.Stop();
            PresenceManager = null;

            // Try to cancel any host announcements we may have had
            await GameStateManager.UnAnnounce();
        }
        #endregion

        #region Core events
        private bool _gotFirstActivation;

        private async void OnOnlineMenuOpened(object sender, OnlineMenuOpenedEventArgs e)
        {
            // Create or resume update timer
            UpdateTimer.StartTimer();
            
            // Create or resume Rich Presence
            PresenceManager ??= new PresenceManager();
            PresenceManager.Start(GameStateManager.Activity);
            
            // Most things only need to be setup once
            if (!_gotFirstActivation)
            {
                _gotFirstActivation = true;

                Log?.Info("Multiplayer / Online menu opened for the first time, setting up.");

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
            
            // Process auto join launch arg
            if (GlobalModState.AutoJoinBssbKey is not null)
            {
                await Task.Delay(250);
                
                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    PresenceManager?.JoinFromSecret(GlobalModState.AutoJoinBssbKey);
                    GlobalModState.AutoJoinBssbKey = null;    
                });
            }
        }

        private void OnOnlineMenuClosed(object sender, EventArgs e)
        {
            // Suspend online-specific features until the menu is reopened
            UpdateTimer.StopTimer();
            PresenceManager?.Stop();
        }
        #endregion
    }
}
