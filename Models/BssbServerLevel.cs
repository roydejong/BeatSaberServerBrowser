namespace ServerBrowser.Models
{
    public class BssbServerLevel : BssbLevel
    {
        public string? SessionGameId;
        public BeatmapDifficulty? Difficulty;
        public GameplayModifiers? Modifiers;
        /// <summary>
        /// Serialized name of the Beatmap characteristic.
        /// </summary>
        public string? Characteristic;

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