namespace ServerBrowser.Models.Enums
{
    public enum BssbDifficulty : int
    {
        All = -1,
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Expert = 3,
        ExpertPlus = 4,
    }
    
    public static class DifficultyUtils
    {
        public static BssbDifficulty ToBssbDifficulty(this BeatmapDifficultyMask mask) => mask switch
        {
            BeatmapDifficultyMask.All => BssbDifficulty.All,
            BeatmapDifficultyMask.Easy => BssbDifficulty.Easy,
            BeatmapDifficultyMask.Normal => BssbDifficulty.Normal,
            BeatmapDifficultyMask.Hard => BssbDifficulty.Hard,
            BeatmapDifficultyMask.Expert => BssbDifficulty.Expert,
            BeatmapDifficultyMask.ExpertPlus => BssbDifficulty.ExpertPlus,
            _ => BssbDifficulty.All
        };
        
        public static BssbDifficulty ToBssbDifficulty(this BeatmapDifficulty mask) => mask switch
        {
            BeatmapDifficulty.Easy => BssbDifficulty.Easy,
            BeatmapDifficulty.Normal => BssbDifficulty.Normal,
            BeatmapDifficulty.Hard => BssbDifficulty.Hard,
            BeatmapDifficulty.Expert => BssbDifficulty.Expert,
            BeatmapDifficulty.ExpertPlus => BssbDifficulty.ExpertPlus,
            _ => BssbDifficulty.All
        };
        
        public static string ToFormattedText(this BssbDifficulty difficulty) => difficulty switch
        {
            BssbDifficulty.All => "<color=#dbbb48>All</color>",
            BssbDifficulty.Easy => "<color=#3cb371>Easy</color>",
            BssbDifficulty.Normal => "<color=#59b0f4>Normal</color>",
            BssbDifficulty.Hard => "<color=#ff6347>Hard</color>",
            BssbDifficulty.Expert => "<color=#bf2a42>Expert</color>",
            BssbDifficulty.ExpertPlus => "<color=#8f48db>Expert+</color>",
            _ => "<color=#bcbdc2>Unknown</color>"
        };
    }
}