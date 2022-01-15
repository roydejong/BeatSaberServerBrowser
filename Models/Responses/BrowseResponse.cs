using System.Collections.Generic;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models.Responses
{
    public class BrowseResponse : JsonObject<BrowseResponse>
    {
        /// <summary>
        /// Total amount of matching lobbies for the request parameters.
        /// </summary>
        public int Count;
        /// <summary>
        /// Current offset from the start of the lobby list, for pagination.
        /// </summary>
        public int Offset;
        /// <summary>
        /// Page size. Maximum amount of lobbies returned in the response.
        /// </summary>
        public int Limit;
        /// <summary>
        /// List of servers.
        /// </summary>
        public List<BssbServer> Lobbies;
        /// <summary>
        /// Optional message of the day set by the BSSB server.
        /// </summary>
        public string? Message;
    }
}