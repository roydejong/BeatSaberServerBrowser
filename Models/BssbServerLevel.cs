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

        public static BssbServerLevel FromDifficultyBeatmap(IDifficultyBeatmap db, GameplayModifiers? modifiers,
            string? characteristic)
        {
            return new BssbServerLevel()
            {
                LevelId = db.level.levelID,
                SongName = db.level.songName,
                SongSubName = db.level.songSubName,
                SongAuthorName = db.level.songAuthorName,
                LevelAuthorName = db.level.levelAuthorName,
                Difficulty = db.difficulty,
                Modifiers = modifiers,
                Characteristic = characteristic 
            };
        }
    }
}