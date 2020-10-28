using LobbyBrowserMod.Ui;
using BeatSaberMarkupLanguage.GameplaySetup;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;
using HarmonyLib;

namespace LobbyBrowserMod
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

            Log.Info("Multiplayer Lobby Browser: initialized by IPA with config");
            Log.Info($" → Lobby announce enabled: {Config.LobbyAnnounceToggle}");

            GameplaySetup.instance.AddTab("Lobby Browser", "LobbyBrowserMod.Ui.LobbyConfigPanel.bsml", LobbyConfigPanel.instance);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");

            Harmony = new HarmonyLib.Harmony("mod.lobbybrowser");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Info($"Harmony patching complete.");
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
        }
    }
}
