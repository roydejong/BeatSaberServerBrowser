using BeatSaberMarkupLanguage.Components;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
            this.text = Game.GameName;

            if (Game.OwnerId == "SERVER_MSG")
            {
                // Special case: the server can inject a fake game here as a message, e.g. to notify that an update is available
                this.text = $"<color=#cf0389>{Game.GameName}</color>";
                this.subtext = Game.SongName;
                this.icon = Sprites.Portal;
                return;
            }

            if (Game.IsModded)
            {
                this.text += " (Modded)";
            }
            else 
            {
                this.text += " <color=#00ff00>(Vanilla)</color>";
            }

            this.subtext = $"[{Game.PlayerCount} / {Game.PlayerLimit}]";

            if (Game.LobbyState == MultiplayerLobbyState.GameRunning && Game.LevelId != null)
            {
                if (!String.IsNullOrEmpty(Game.SongAuthor))
                {
                    this.subtext += $" {Game.SongAuthor} -";
                }

                this.subtext += $" {Game.SongName}";
            }
            else
            {
                this.subtext += $" In lobby";
            }

            if (Game.Difficulty.HasValue && !String.IsNullOrEmpty(Game.LevelId))
            {
                this.subtext += $" ({Game.Difficulty})";
            }

            try
            {
                SetCoverArt();
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"Could not set cover art for level {Game.LevelId}: {ex}");
            }
        }

        private async Task<bool> SetCoverArt()
        {
            if (String.IsNullOrEmpty(Game.LevelId) || Game.LobbyState != MultiplayerLobbyState.GameRunning) 
            {
                // No level info / we are in a lobby
                UpdateIcon(Sprites.PortalUser);
                return true;
            }

            var coverArtSprite = await CoverArtGrabber.GetCoverArtSprite(Game, _cancellationTokenSource.Token);

            if (coverArtSprite != null)
            {
                // Official level, or installed custom level found
                UpdateIcon(coverArtSprite);
                return true;
            }

            // Failed to get level info, show beatsaver icon as placeholder
            UpdateIcon(Sprites.BeatSaverIcon);
            return false;
        }

        private void UpdateIcon(Sprite nextIcon)
        {
            if (this.icon == null || this.icon.name != nextIcon.name)
            {
                this.icon = nextIcon;
                _onContentChange(this);
            }
        }
    }
}
