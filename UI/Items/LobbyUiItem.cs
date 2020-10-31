using BeatSaberMarkupLanguage.Components;
using ServerBrowser.Assets;
using ServerBrowser.Core;

namespace ServerBrowser.UI.Items
{
    public class LobbyUiItem : CustomListTableData.CustomCellInfo
    {
        public HostedGameData LobbyInfo
        {
            get;
            private set;
        }

        public LobbyUiItem(HostedGameData lobbyInfo) : base("Lobby", "Lobby info", Sprites.BeatSaverIcon)
        {
            LobbyInfo = lobbyInfo;

            UpdateUi();
        }

        public void UpdateUi()
        {
            var modeDescription = LobbyInfo.IsModded ? "Modded" : "Vanilla";

            this.text = LobbyInfo.GameName;

            if (LobbyInfo.LobbyState == MultiplayerLobbyState.GameRunning && LobbyInfo.LevelId != null)
            {
                this.subtext = $"[{LobbyInfo.PlayerCount} / {LobbyInfo.PlayerLimit}] {LobbyInfo.SongAuthor} - {LobbyInfo.SongName} ({LobbyInfo.Difficulty})";
                SetCoverArt(LobbyInfo.LevelId);
            }
            else
            {
                this.subtext = $"[{LobbyInfo.PlayerCount} / {LobbyInfo.PlayerLimit}] {modeDescription} lobby";
            }
        }

        public void SetCoverArt(string levelId)
        {
            // TODO Load cover art via game, or BeatSaver, somehow.
        }
    }
}
