using LobbyBrowserMod.Harmony;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Linq;
using UnityEngine;
using TMPro;
using System;

namespace LobbyBrowserMod.Ui
{
    internal class LobbyConfigPanel : NotifiableSingleton<LobbyConfigPanel>
    {
        #region LobbyAnnounceToggle
        [UIComponent("LobbyAnnounceToggle")]
        public ToggleSetting lobbyAnnounceToggle;

        [UIValue("LobbyAnnounceToggle")]
        public bool LobbyAnnounceToggleValue
        {
            get
            {
                return Plugin.Config.LobbyAnnounceToggle;
            }

            set
            {
                Plugin.Config.LobbyAnnounceToggle = value;
            }
        }

        [UIAction("LobbyAnnounceToggle")]
        public void SetLobbyAnnounceToggle(bool value)
        {
            LobbyAnnounceToggleValue = value;
            UpdateState();
        }
        #endregion

        #region StatusText
        [UIComponent("StatusText")]
        public TextMeshProUGUI statusText;
        #endregion

        #region UI Update
        private MultiplayerSessionManager sessionManager = null;

        public void UpdateState()
        {
            if (!LobbyJoinPatch.IsPartyMultiplayer)
            {
                statusText.text = "Only supported for custom multiplayer games.";
                statusText.color = Color.yellow;

                lobbyAnnounceToggle.interactable = false;
                lobbyAnnounceToggle.Value = false;
                return;
            }

            sessionManager = Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().First();

            if (!LobbyJoinPatch.IsPartyHost)
            {
                // We are not the host
                lobbyAnnounceToggle.interactable = false;

                var theHost = sessionManager.connectionOwner;

                if (theHost != null && theHost.HasState("lobbyannounce"))
                {
                    lobbyAnnounceToggle.Value = true;

                    statusText.text = "The host has announced this lobby.";
                    statusText.color = Color.green;
                }
                else
                {
                    lobbyAnnounceToggle.Value = false;

                    statusText.text = "The host has not announced this lobby.";
                    statusText.color = Color.red;
                }

                return;
            }

            // We are the host, enable controls
            sessionManager.SetLocalPlayerState("lobbyannounce", LobbyAnnounceToggleValue);

            lobbyAnnounceToggle.interactable = true;
            lobbyAnnounceToggle.Value = LobbyAnnounceToggleValue;

            if (!LobbyAnnounceToggleValue)
            {
                // Currently disabled
                statusText.text = "Flip the switch to share your Server Code to the world.";
                statusText.color = Color.gray;
                return;
            }

            // Currently enabled
            statusText.text = "Announcing...";
            statusText.color = Color.green;
        }
        #endregion
    }
}