using HarmonyLib;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.Game.Models;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us determine which song is being played, on start of the mp level.
    /// </summary>
    [HarmonyPatch(typeof(LobbyGameStateController), "StartMultiplayerLevel", MethodType.Normal)]
    public static class StartMultiplayerLevelPatch
    {
        public static void Postfix(IPreviewBeatmapLevel previewBeatmapLevel, BeatmapDifficulty beatmapDifficulty,
            BeatmapCharacteristicSO beatmapCharacteristic, GameplayModifiers gameplayModifiers,
            LobbyGameStateController __instance)
        {
            MpEvents.RaiseStartingMultiplayerLevel(__instance, new StartingMultiplayerLevelEventArgs()
            {
                BeatmapLevel = previewBeatmapLevel,
                Difficulty = beatmapDifficulty,
                Characteristic = beatmapCharacteristic,
                Modifiers = gameplayModifiers
            });
        }
    }
}
