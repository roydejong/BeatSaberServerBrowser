using System.Collections.Generic;
using IPA.Loader;
using JetBrains.Annotations;

namespace ServerBrowser.Utils
{
    public class ModChecker
    {
        private static Dictionary<string, PluginMetadata>  _pluginMetadata = new();

        private static PluginMetadata GetPluginMetadata(string pluginId)
        {
            if (!_pluginMetadata.ContainsKey(pluginId))
                _pluginMetadata[pluginId] = PluginManager.GetPluginFromId(pluginId);
            return _pluginMetadata[pluginId];
        }

        /// <summary>
        /// Gets whether or not a plugin is installed and enabled.
        /// </summary>
        public static bool GetIsEnabled(string pluginId)
        {
            var pluginMetadata = GetPluginMetadata(pluginId);
            return pluginMetadata != null && PluginManager.IsEnabled(pluginMetadata);
        }

        /// <summary>
        /// Gets whether or not a plugin is installed, enabled, and of a minimum version.
        /// </summary>
        public static bool GetIsMinVersion(string pluginId, SemVer.Version minVersion)
        {
            return GetIsEnabled(pluginId) && GetPluginMetadata(pluginId).Version >= minVersion;
        }

        /// <summary>
        /// Gets the version of the a plugin, or null if it is not installed.
        /// </summary>
        [CanBeNull]
        public static SemVer.Version GetInstalledVersion(string pluginId)
        {
            return GetPluginMetadata(pluginId)?.Version;
        }

        public class MultiplayerExtensions
        {
            public const string PluginId = "MultiplayerExtensions";
            public static bool InstalledAndEnabled => ModChecker.GetIsEnabled(PluginId);
            [CanBeNull] public static SemVer.Version InstalledVersion => ModChecker.GetPluginMetadata(PluginId)?.Version;
        }

        public class DiscordCore
        {
            public const string PluginId = "DiscordCore";
            public static bool InstalledAndEnabled => ModChecker.GetIsEnabled(PluginId);
            [CanBeNull] public static SemVer.Version InstalledVersion => ModChecker.GetPluginMetadata(PluginId)?.Version;
        }
    }
}