namespace ServerBrowser
{
    public class PluginConfig
    {
        public bool LobbyAnnounceToggle { get; set; } = true;
        public string CustomGameName { get; set; } = null;
        public bool JoinNotificationsEnabled { get; set; } = true;
        public bool UseNativeBrowserPreview { get; set; } = false;
    }
}