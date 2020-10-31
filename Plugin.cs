using ServerBrowser.UI;
using BeatSaberMarkupLanguage.GameplaySetup;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;
using HarmonyLib;
using ServerBrowser.Core;
using ServerBrowser.Assets;

namespace ServerBrowser
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static PluginConfig Config { get; private set; }
        internal static HarmonyLib.Harmony Harmony { get; private set; }

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

            // TODO one day figure out how to not do this in single player, doesn't currently seem possible to remove it conditionally though
            LobbyConfigPanel.RegisterGameplayModifierTab();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log?.Debug("OnApplicationStart");

            Harmony = new HarmonyLib.Harmony("mod.lobbybrowser");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log?.Debug($"Harmony patching complete.");

            Sprites.Initialize();
            Log?.Debug($"Sprite conversion complete.");
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log?.Debug("OnApplicationQuit");

            // Try to cancel any host announcements we may have had
            LobbyStateManager.UnAnnounce();
        }
    }
}
