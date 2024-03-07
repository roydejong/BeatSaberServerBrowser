namespace ServerBrowser
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbConfig
    {
        /// <summary>
        /// The base URL for the BSSB API.
        /// </summary>
        public virtual string ApiServerUrl { get; set; } = "https://bssb.app/";
        
        /// <summary>
        /// Indicates whether the user has accepted the privacy disclaimer, and which version they accepted.
        /// </summary>
        public virtual uint AcceptedPrivacyDisclaimerVersion { get; set; } = 0;
        
        internal bool AnyPrivacyDisclaimerAccepted => AcceptedPrivacyDisclaimerVersion > 0;
    }
}