namespace ServerBrowser.Models.Enums
{
    public enum BssbLobbyDifficulty : int
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
        public static BssbLobbyDifficulty ToBssbLobbyDifficulty(this BeatmapDifficultyMask mask) => mask switch
        {
            BeatmapDifficultyMask.All => BssbLobbyDifficulty.All,
            BeatmapDifficultyMask.Easy => BssbLobbyDifficulty.Easy,
            BeatmapDifficultyMask.Normal => BssbLobbyDifficulty.Normal,
            BeatmapDifficultyMask.Hard => BssbLobbyDifficulty.Hard,
            BeatmapDifficultyMask.Expert => BssbLobbyDifficulty.Expert,
            BeatmapDifficultyMask.ExpertPlus => BssbLobbyDifficulty.ExpertPlus,
            _ => BssbLobbyDifficulty.All
        };
        
        public static BssbLobbyDifficulty ToBssbLobbyDifficulty(this BeatmapDifficulty mask) => mask switch
        {
            BeatmapDifficulty.Easy => BssbLobbyDifficulty.Easy,
            BeatmapDifficulty.Normal => BssbLobbyDifficulty.Normal,
            BeatmapDifficulty.Hard => BssbLobbyDifficulty.Hard,
            BeatmapDifficulty.Expert => BssbLobbyDifficulty.Expert,
            BeatmapDifficulty.ExpertPlus => BssbLobbyDifficulty.ExpertPlus,
            _ => BssbLobbyDifficulty.All
        };
    }
}