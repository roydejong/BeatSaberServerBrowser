using BeatSaberMarkupLanguage.Components;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBrowser.UI.Items
{
    public class HostedGameCell : CustomListTableData.CustomCellInfo
    {
        public HostedGameData Game
        {
            get;
            private set;
        }

        private static CancellationTokenSource _cancellationTokenSource;
        private static Action<HostedGameCell> _onContentChange;

        public HostedGameCell(CancellationTokenSource cancellationTokenSource, Action<HostedGameCell> onContentChange, HostedGameData game)
            : base("A game", "Getting details...", Sprites.BeatSaverIcon)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _onContentChange = onContentChange;

            Game = game;

            UpdateUi();
        }

        public void UpdateUi()
        {
            var modeDescription = Game.IsModded ? "Modded" : "Vanilla";

            this.text = Game.GameName;

            if (Game.LobbyState == MultiplayerLobbyState.GameRunning && Game.LevelId != null)
            {
                this.subtext = $"[{Game.PlayerCount} / {Game.PlayerLimit}] {Game.SongAuthor} - {Game.SongName} ({Game.Difficulty})";

                try
                {
                    SetCoverArt(Game.LevelId);
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"Could not set cover art for level {Game.LevelId}: {ex}");
                }
            }
            else
            {
                this.subtext = $"[{Game.PlayerCount} / {Game.PlayerLimit}] {modeDescription} lobby";
            }
        }

        private async Task<bool> SetCoverArt(string levelId)
        {
            var levelHash = SongCore.Collections.hashForLevelID(levelId);
            var levelIsInstalled = SongCore.Collections.songWithHashPresent(levelHash);

            Plugin.Log?.Warn($"levelId: {levelId}, levelHash: {levelHash}, levelIsInstalled: {levelIsInstalled}");

            if (levelIsInstalled)
            {
                // Native level, or installed custom level
                var level = SongCore.Loader.GetLevelById(levelId);
                Plugin.Log?.Warn($"??? 1");
                this.icon = await level.GetCoverImageAsync(_cancellationTokenSource.Token);
                Plugin.Log?.Warn($"cover art set! _onContentChange invoke...");
                _onContentChange(this);
                return true;
            }

            Plugin.Log?.Warn($"??? 2");

            // TODO Download from beat saver if needed
            // TODO Test with official
            // TODO Test with installed custom

            return false;
        }

        private async void SetImageFromBeatSaverSong(BeatSaverSharp.Beatmap song)
        {
            var imageBytes = await song.FetchCoverImage();
            var icon = Sprites.LoadSpriteRaw(imageBytes);

            this.icon = icon;
        }
    }
}
