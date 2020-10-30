using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using LobbyBrowserMod.Assets;
using LobbyBrowserMod.Core;
using LobbyBrowserMod.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyBrowserMod.UI.Items
{
    public class LobbyUiItem : CustomListTableData.CustomCellInfo
    {
        public LobbyAnnounceInfo LobbyInfo
        {
            get;
            private set;
        }

        public LobbyUiItem(LobbyAnnounceInfo lobbyInfo) : base("Lobby", "Lobby info", Sprites.BeatSaverIcon)
        {
            LobbyInfo = lobbyInfo;

            UpdateUi();
        }

        public void UpdateUi()
        {
            var modeDescription = LobbyInfo.IsModded ? "Modded" : "Vanilla";

            this.text = LobbyInfo.GameName;
            this.subtext = $"{LobbyInfo.PlayerCount} / {LobbyInfo.PlayerLimit} players, {modeDescription}";
        }
    }
}
