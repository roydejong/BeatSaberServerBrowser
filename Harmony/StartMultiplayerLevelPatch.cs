using HarmonyLib;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.UI;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us determine which song is being played, on start of the mp level.
    /// </summary>
    [HarmonyPatch(typeof(LobbyGameStateController), "StartMultiplayerLevel", MethodType.Normal)]
    class StartMultiplayerLevelPatch
    {
        public static void Postfix(IPreviewBeatmapLevel previewBeatmapLevel, BeatmapDifficulty beatmapDifficulty,
            BeatmapCharacteristicSO beatmapCharacteristic, GameplayModifiers gameplayModifiers)
        {
            Plugin.Log?.Debug($"StartMultiplayerLevel: [{previewBeatmapLevel.levelID}] {previewBeatmapLevel.songAuthorName} - {previewBeatmapLevel.songName} on {beatmapDifficulty}");
            GameStateManager.HandleSongSelected(previewBeatmapLevel, beatmapDifficulty, beatmapCharacteristic, gameplayModifiers);

            if (Plugin.Config.JoinNotificationsEnabled)
            {
                FloatingNotification.Instance.ShowMessage(
                    "Starting multiplayer level",
                    $"{previewBeatmapLevel.songAuthorName} {previewBeatmapLevel.songName} ({beatmapDifficulty})",
                    FloatingNotification.NotificationStyle.Yellow,
                    Sprites.BeatSaverIcon
                );
            }
        }
    }
}
