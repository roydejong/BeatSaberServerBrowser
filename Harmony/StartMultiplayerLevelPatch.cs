using HarmonyLib;
using ServerBrowser.Core;
using ServerBrowser.UI;
using System;

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
            Plugin.Log?.Info($"StartMultiplayerLevel: [{previewBeatmapLevel.levelID}] {previewBeatmapLevel.songAuthorName} - {previewBeatmapLevel.songName} on {beatmapDifficulty}");
            GameStateManager.HandleSongSelected(previewBeatmapLevel, beatmapDifficulty, beatmapCharacteristic, gameplayModifiers);

            FloatingNotification.Instance.ShowMessage("StartMultiplayerLevel", previewBeatmapLevel.songName);
        }
    }
}
