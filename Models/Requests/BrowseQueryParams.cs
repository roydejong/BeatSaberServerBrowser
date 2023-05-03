using ServerBrowser.Utils;

namespace ServerBrowser.Models.Requests
{
    public class BrowseQueryParams
    {
        public int Offset;
        public string? TextSearch;
        public bool HideFullGames;
        public bool HideModdedGames;
        public bool HideVanillaGames;
        public bool HideInProgressGames;
        public bool HideQuickPlay;

        /// <summary>
        /// Indicates whether any filters have been activated.
        /// </summary>
        public bool AnyFiltersActive =>
            !string.IsNullOrEmpty(TextSearch) || HideFullGames || HideModdedGames || HideVanillaGames
            || HideInProgressGames || HideQuickPlay;

        public void Reset()
        {
            TextSearch = null;
            HideFullGames = false;
            HideModdedGames = false;
            HideVanillaGames = false;
            HideInProgressGames = false;
            HideQuickPlay = false;
        }

        public string ToQueryString()
        {
            var queryString = new QueryString();

            queryString.Set("limit", 7.ToString());
            queryString.Set("includeLevel", "1");
            
            if (Offset > 0)
                queryString.Set("offset", Offset.ToString());
            
            if (!string.IsNullOrEmpty(TextSearch))
                queryString.Set("query", TextSearch);
            
            if (HideFullGames)
                queryString.Set("filterFull", "1");
            
            if (HideModdedGames)
                queryString.Set("filterModded", "1");
            else if (HideVanillaGames)
                queryString.Set("filterVanilla", "1");
            
            if (HideInProgressGames)
                queryString.Set("filterInProgress", "1");
            
            if (HideQuickPlay)
                queryString.Set("filterQuickPlay", "1");

            return queryString.ToString();
        }
    }
}