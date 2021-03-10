using System;

namespace ServerBrowser.Core
{
    public class HostedGameFilters
    {
        public string TextSearch { get; set; } = null;
        public bool HideFullGames { get; set; } = false;
        public bool HideModdedGames { get; set; } = false;
        public bool HideInProgressGames { get; set; } = false;

        public void Reset()
        {
            TextSearch = null;
            HideFullGames = false;
            HideModdedGames = false;
            HideInProgressGames = false;
        }

        /// <summary>
        /// Indicates whether any filters have been activated.
        /// </summary>
        public bool AnyActive => !String.IsNullOrEmpty(TextSearch) || HideFullGames || HideModdedGames || HideInProgressGames;
    }
}
