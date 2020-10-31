using BeatSaberMarkupLanguage.Components;
using BeatSaverSharp;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ServerBrowser.UI.Items
{
    public class HostedGameCell : CustomListTableData.CustomCellInfo
    {
        public HostedGameData Game
        {
            get;
            private set;
        }

        public bool IsShowingInGame
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

                IsShowingInGame = true;
            }
            else
            {
                this.subtext = $"[{Game.PlayerCount} / {Game.PlayerLimit}] {modeDescription} lobby";

                IsShowingInGame = false;
            }
        }

        private async Task<bool> SetCoverArt(string levelId)
        {
            var levelIsCustom = levelId.StartsWith("custom_", StringComparison.InvariantCultureIgnoreCase);

            if (levelIsCustom)
            {
                var levelHash = SongCore.Collections.hashForLevelID(levelId);
                var levelIsInstalled = SongCore.Collections.songWithHashPresent(levelHash);

                if (levelIsInstalled)
                {
                    // Custom level - installed
                    var level = SongCore.Loader.GetLevelByHash(levelHash);

                    this.icon = await level.GetCoverImageAsync(_cancellationTokenSource.Token);
                    _onContentChange(this);
                    return true;
                }
                else
                {
                    // Custom level - not installed
                    BeatSaverSharp.Beatmap beatSaverLevel = new BeatSaverSharp.Beatmap(Plugin.BeatSaver, levelHash);

                    if (beatSaverLevel != null)
                    {
                        this.icon = Sprites.LoadSpriteRaw(await beatSaverLevel.FetchCoverImage());
                        _onContentChange(this);
                        return true;
                    }
                }
            }
            else
            {
                // Official level
                var level = SongCore.Loader.GetLevelById(levelId);

                this.icon = await level.GetCoverImageAsync(_cancellationTokenSource.Token);
                _onContentChange(this);
                return true;
            }
            
            // Failed to get level info, can't set cover art, too bad, very sad
            return false;
        }
    }
}
