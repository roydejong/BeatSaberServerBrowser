using System.Collections.Generic;
using Hive.Versioning;
using IPA.Loader;

namespace ServerBrowser.Utils
{
    internal static class ModCheck
    {
        private static Dictionary<string, PluginMetadata> _pluginMetadata = new();

        internal static PluginMetadata? GetPluginMetadata(string pluginId)
        {
            if (!_pluginMetadata.ContainsKey(pluginId))
                _pluginMetadata[pluginId] = PluginManager.GetPluginFromId(pluginId);

            return _pluginMetadata[pluginId];
        }

        /// <summary>
        /// Gets whether or not a plugin is installed and enabled.
        /// </summary>
        internal static bool GetIsEnabled(string pluginId)
        {
            var pluginMetadata = GetPluginMetadata(pluginId);
            return pluginMetadata != null && PluginManager.IsEnabled(pluginMetadata);
        }

        /// <summary>
        /// Gets whether or not a plugin is installed, enabled, and of a minimum version.
        /// </summary>
        internal static bool GetIsMinVersion(string pluginId, Version minVersion) =>
            GetIsEnabled(pluginId) && GetPluginMetadata(pluginId)!.HVersion >= minVersion;

        /// <summary>
        /// Gets the version of the a plugin, or null if it is not installed.
        /// </summary>
        internal static Version? GetInstalledVersion(string pluginId) => GetPluginMetadata(pluginId)?.HVersion;

        internal class MultiplayerCore
        {
            internal const string PluginId = "MultiplayerCore";
            internal static bool InstalledAndEnabled => ModCheck.GetIsEnabled(PluginId);
            internal static Version? InstalledVersion => ModCheck.GetPluginMetadata(PluginId)?.HVersion;
        }
    }
}