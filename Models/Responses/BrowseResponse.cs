using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models.Responses
{
    public class BrowseResponse : JsonObject<BrowseResponse>
    {
        /// <summary>
        /// Total amount of matching lobbies for the request parameters.
        /// </summary>
        [JsonProperty("Count")] public int TotalResultCount;
        /// <summary>
        /// Current offset from the start of the lobby list, for pagination.
        /// </summary>
        [JsonProperty("Offset")] public int PageOffset;
        /// <summary>
        /// Page size. Maximum amount of lobbies returned in the response.
        /// </summary>
        [JsonProperty("Limit")] public int PageSize;
        /// <summary>
        /// List of servers.
        /// </summary>
        [JsonProperty("Lobbies")] public List<BssbServer>? Servers;
        /// <summary>
        /// Optional message of the day set by the BSSB server.
        /// </summary>
        [JsonProperty("Message")] public string? MessageOfTheDay;

        [JsonIgnore] public int PageCount => (int) Math.Ceiling(TotalResultCount / (decimal) PageSize);
    }
}