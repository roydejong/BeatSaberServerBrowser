using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ServerBrowser.Models
{
    public class BssbLevel
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

        [JsonProperty("SessionGameId")] public string? SessionGameId;
        [JsonProperty("Difficulty")] public BeatmapDifficulty? Difficulty;
        [JsonProperty("Modifiers")] public GameplayModifiers? Modifiers;

        /// <summary>
        /// Serialized name of the Beatmap characteristic.
        /// </summary>
        [JsonProperty("Characteristic")] public string? Characteristic;
        
        [JsonIgnore]
        public string CharacteristicText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Characteristic))
                    return "Standard";
                
                // To string with spaces
                return Regex.Replace(Characteristic!, "(\\B[A-Z])", " $1");
            }
        }
    }
}