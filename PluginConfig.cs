namespace ServerBrowser
{
    public class PluginConfig
    {
        /// <summary>
        /// If enabled, when hosting a custom lobby, it will be shared to the server browser.
        /// This can be toggled from the "Create Server" dialog and the in-lobby modifiers panel.
        /// </summary>
        public bool LobbyAnnounceToggle { get; set; } = true;
        
        /// <summary>
        /// If enabled, when joining a Quick Play lobby, it will be shared to the server browser.
        /// This can be toggled from the in-lobby modifiers panel.
        /// </summary>
        public bool ShareQuickPlayGames { get; set; } = true;
        
        /// <summary>
        /// If a value is set, this name will be used when announcing the game to the server browser and master server. 
        /// </summary>
        public string CustomGameName { get; set; } = null;
        
        /// <summary>
        /// If enabled, overhead join/leave notifications will appear in the lobby and in multiplayer games.
        /// This can be toggled from the in-lobby modifiers panel.
        /// </summary>
        public bool JoinNotificationsEnabled { get; set; } = true;

        /// <summary>
        /// This setting controls whether the Discord Rich Presence feature is globally enabled or not.
        /// </summary>
        public bool DiscordRichPresenceEnabled { get; set; } = true;

        /// <summary>
        /// This setting controls whether the Steam Rich Presence feature is globally enabled or not.
        /// </summary>
        public bool SteamRichPresenceEnabled { get; set; } = true;
    }
}