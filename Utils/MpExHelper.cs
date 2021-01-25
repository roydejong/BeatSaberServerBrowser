using IPA.Loader;

namespace ServerBrowser.Utils
{
    public static class MpExHelper
    {
        private const string PLUGIN_ID = "MultiplayerExtensions";

        private static PluginMetadata _pluginMetadata;

        private static PluginMetadata GetPluginMetadata()
        {
            if (_pluginMetadata == null)
            {
                _pluginMetadata = PluginManager.GetPluginFromId(PLUGIN_ID);
            }

            return _pluginMetadata;
        }

        /// <summary>
        /// Gets whether or not the MultiplayerExtensions plugin is installed and enabled.
        /// </summary>
        public static bool GetIsInstalled()
        {
            var pluginMetadata = GetPluginMetadata();

            return pluginMetadata != null
                && PluginManager.IsEnabled(pluginMetadata);
        }

        /// <summary>
        /// Gets the version of the installed MultiplayerExtensions plugin, or null if it is not installed.
        /// </summary>
        public static SemVer.Version GetInstalledVersion()
        {
            return GetPluginMetadata()?.Version;
        }
    }
}
