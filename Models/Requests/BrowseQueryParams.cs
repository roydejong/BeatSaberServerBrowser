using System;
using System.Web;

namespace ServerBrowser.Models.Requests
{
    public class BrowseQueryParams
    {
        public int Offset;
        public string? TextSearch;
        public bool HideFullGames;
        public bool HideModdedGames;
        public bool HideInProgressGames;

        /// <summary>
        /// Indicates whether any filters have been activated.
        /// </summary>
        public bool AnyFiltersActive =>
            !String.IsNullOrEmpty(TextSearch) || HideFullGames || HideModdedGames || HideInProgressGames;

        public void Reset()
        {
            TextSearch = null;
            HideFullGames = false;
            HideModdedGames = false;
            HideInProgressGames = false;
        }

        public string ToQueryString()
        {
            var queryString = HttpUtility.ParseQueryString("");

            queryString.Add("limit", 7.ToString());
            queryString.Add("includeLevel", "1");
            
            if (Offset > 0)
                queryString.Add("offset", Offset.ToString());
            
            if (!String.IsNullOrEmpty(TextSearch))
                queryString.Add("query", TextSearch);
            
            if (HideFullGames)
                queryString.Add("filterFull", "1");
            
            if (HideModdedGames)
                queryString.Add("filterModded", "1");
            
            if (HideInProgressGames)
                queryString.Add("filterInProgress", "1");

            return queryString.ToString();
        }
    }
}