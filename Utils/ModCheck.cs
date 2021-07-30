using System.Collections.Generic;
using Hive.Versioning;
using IPA.Loader;
using JetBrains.Annotations;

namespace ServerBrowser.Utils
{
    public class ModCheck
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
        public static bool GetIsMinVersion(string pluginId, Version minVersion)
        {
            return GetIsEnabled(pluginId) && GetPluginMetadata(pluginId).HVersion >= minVersion;
        }

        /// <summary>
        /// Gets the version of the a plugin, or null if it is not installed.
        /// </summary>
        [CanBeNull]
        public static Version GetInstalledVersion(string pluginId)
        {
            return GetPluginMetadata(pluginId)?.HVersion;
        }

        public class MultiplayerExtensions
        {
            public const string PluginId = "MultiplayerExtensions";
            public static bool InstalledAndEnabled => ModCheck.GetIsEnabled(PluginId);
            public static Version? InstalledVersion => ModCheck.GetPluginMetadata(PluginId)?.HVersion;
        }

        public class DiscordCore
        {
            public const string PluginId = "DiscordCore";
            public static bool InstalledAndEnabled => ModCheck.GetIsEnabled(PluginId);
            public static Version? InstalledVersion => ModCheck.GetPluginMetadata(PluginId)?.HVersion;
        }
    }
}