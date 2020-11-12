using BeatSaberMarkupLanguage.Components;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBrowser.UI.Components
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
            this.subtext = $"[{Game.PlayerCount} / {Game.PlayerLimit}]";

            if (Game.LobbyState == MultiplayerLobbyState.GameRunning && Game.LevelId != null)
            {
                if (!String.IsNullOrEmpty(Game.SongAuthor))
                {
                    this.subtext += $" {Game.SongAuthor} -";
                }

                this.subtext += $" {Game.SongName}";

                try
                {
                    SetCoverArt();
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"Could not set cover art for level {Game.LevelId}: {ex}");
                }
            }
            else
            {
                this.subtext += $" {modeDescription} lobby";
            }

            if (Game.Difficulty.HasValue && !String.IsNullOrEmpty(Game.LevelId))
            {
                this.subtext += $" ({Game.Difficulty})";
            }
        }

        private async Task<bool> SetCoverArt()
        {
            var coverArtSprite = await CoverArtGrabber.GetCoverArtSprite(Game, _cancellationTokenSource.Token);

            if (coverArtSprite != null)
            {
                this.icon = coverArtSprite;
                _onContentChange(this);
                return true;
            }

            return false;
        }
    }
}
