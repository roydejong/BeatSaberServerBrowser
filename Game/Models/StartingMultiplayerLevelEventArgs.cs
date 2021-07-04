namespace ServerBrowser.Game.Models
{
    public struct StartingMultiplayerLevelEventArgs
    {
        public IPreviewBeatmapLevel BeatmapLevel;
        public BeatmapDifficulty Difficulty;
        public BeatmapCharacteristicSO Characteristic;
        public GameplayModifiers Modifiers;
    }
}