namespace ServerBrowser
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PluginConfig
    {
        /// <summary>
        /// The base URL for the BSSB API.
        /// </summary>
        public virtual string ApiServerUrl { get; set; } = "https://bssb.app/api/";
        
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
    }
}