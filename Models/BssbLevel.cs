using Newtonsoft.Json;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models
{
    public class BssbLevel : JsonObject<BssbLevel>
    {
        [JsonProperty("LevelId")] public string? LevelId;
        [JsonProperty("SongName")] public string? SongName;
        [JsonProperty("SongSubName")] public string? SongSubName;
        [JsonProperty("SongAuthorName")] public string? SongAuthorName;
        [JsonProperty("LevelAuthorName")] public string? LevelAuthorName;
        /// <summary>
        /// HTTP URL for the cover art associated with the level, if any.
        /// Provided by the API when querying lobbies.
        /// </summary>
        [JsonProperty("CoverUrl")] public string? CoverArtUrl;
    }
}