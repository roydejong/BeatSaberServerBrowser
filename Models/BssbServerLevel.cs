using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ServerBrowser.Models
{
    public class BssbServerLevel : BssbLevel
    {
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

        public static BssbServerLevel FromLevelStartData(BeatmapLevel level, BeatmapDifficulty difficulty, 
            GameplayModifiers? modifiers = null, string? characteristic = null)
        {
            return new BssbServerLevel()
            {
                LevelId = level.levelID,
                SongName = level.songName,
                SongSubName = level.songSubName,
                SongAuthorName = level.songAuthorName,
                Difficulty = difficulty,
                Modifiers = modifiers,
                Characteristic = characteristic
            };
        }
    }
}