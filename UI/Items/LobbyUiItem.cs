using BeatSaberMarkupLanguage.Attributes;
using LobbyBrowserMod.Core;
using LobbyBrowserMod.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyBrowserMod.UI.Items
{
    public class LobbyUiItem
    {
        private LobbyAnnounceInfo _lobbyInfo;

        [UIComponent("gameTitle")]
        private TextMeshProUGUI _gameTitle;

        [UIComponent("gameSubTitle")]
        private TextMeshProUGUI _gameSubTitle;

        [UIComponent("joinButton")]
        private Button _joinButton;

        public LobbyUiItem(LobbyAnnounceInfo lobbyInfo)
        {
            _lobbyInfo = lobbyInfo;
        }

        public void UpdateUi()
        {
            var modeDescription = _lobbyInfo.IsModded ? "Modded" : "Vanilla";

            _gameTitle.text = _lobbyInfo.GameName;
            _gameSubTitle.text = $"{_lobbyInfo.PlayerCount} / {_lobbyInfo.PlayerLimit} players, {modeDescription}";

            _joinButton.interactable = !string.IsNullOrEmpty(_lobbyInfo.ServerCode);
        }

        [UIAction("#post-parse")]
        internal void OnItemLoaded()
        {
            UpdateUi();
        }


        [UIAction("joinClicked")]
        internal void JoinClicked()
        {
            Plugin.Log?.Info($"Requested join for lobby: {_lobbyInfo.GameName} / {_lobbyInfo.ServerCode}");
            GameMp.JoinLobbyWithCode(_lobbyInfo.ServerCode);
        }
    }
}
