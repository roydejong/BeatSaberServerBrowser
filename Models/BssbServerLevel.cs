namespace ServerBrowser.Models
{
    public class BssbServerLevel : BssbLevel
    {
        public string? SessionGameId;
        public BeatmapDifficulty? Difficulty;
        /// <summary>
        /// Serialized name of the Beatmap characteristic.
        /// </summary>
        public string? Characteristic;

        public static BssbServerLevel FromDifficultyBeatmap(IDifficultyBeatmap db)
        {
            return new BssbServerLevel()
            {
                LevelId = db.level.levelID,
                SongName = db.level.songName,
                SongSubName = db.level.songSubName,
                SongAuthorName = db.level.songAuthorName,
                LevelAuthorName = db.level.levelAuthorName,
                Difficulty = db.difficulty
            };
        }
    }
}