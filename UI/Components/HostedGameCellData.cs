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
    public class HostedGameCellData : CustomListTableData.CustomCellInfo
    {
        public HostedGameData Game
        {
            get;
            private set;
        }

        private static CancellationTokenSource _cancellationTokenSource;
        private static Action<HostedGameCellData> _onContentChange;

        public HostedGameCellData(CancellationTokenSource cancellationTokenSource, Action<HostedGameCellData> onContentChange, HostedGameData game)
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

            if (Game.IsOnCustomMaster)
            {
                this.text += $" <color=#59b0f4><size=3>({Game.MasterServerHost})</size></color>";
            }

            this.subtext = $"";

            if (Game.LobbyState == MultiplayerLobbyState.GameRunning && Game.LevelId != null)
            {
                if (!String.IsNullOrEmpty(Game.SongAuthor))
                {
                    this.subtext += $"{Game.SongAuthor} - ";
                }

                this.subtext += $"{Game.SongName}";
            }
            else
            {
                this.subtext += $"In lobby";
            }

            if (Game.Difficulty.HasValue && !String.IsNullOrEmpty(Game.LevelId))
            {
                this.subtext += $" ({Game.DescribeDifficulty(true)})";
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
