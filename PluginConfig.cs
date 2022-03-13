namespace ServerBrowser
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PluginConfig
    {
        /// <summary>
        /// The base URL for the BSSB API.
        /// </summary>
        public virtual string ApiServerUrl { get; set; } = "https://bssb.app/";
        
        /// <summary>
        /// If true, send server announcements when you are the party leader.
        /// </summary>
        public virtual bool AnnounceParty { get; set; } = true;

        /// <summary>
        /// If true, send Quick Play server announcements.
        /// </summary>
        public virtual bool AnnounceQuickPlay { get; set; } = true;

        /// <summary>
        /// Custom server name to use with party leader announcements.
        /// </summary>
        public virtual string? ServerName { get; set; } = null;

        /// <summary>
        /// Controls whether the JoiningLobbyExtender patches are applied.
        /// If enabled, extended connection status is shown during connect (English only).
        /// </summary>
        public virtual bool EnableJoiningLobbyExtender { get; set; } = true;
    }
}