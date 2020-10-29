using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using LobbyBrowserMod.Core;
using LobbyBrowserMod.Harmony;
using LobbyBrowserMod.Utils;
using TMPro;
using UnityEngine;

namespace LobbyBrowserMod.UI
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
            LobbyStateManager.HandleUpdate();
        }
        #endregion

        #region StatusText
        [UIComponent("StatusText")]
        public TextMeshProUGUI statusText;
        #endregion

        #region UI Update
        private MultiplayerSessionManager sessionManager = null;

        public static void UpdatePanelInstance()
        {
            if (LobbyConfigPanel.instance != null)
            {
                LobbyConfigPanel.instance.UpdatePanel();
            }
        }

        public void UpdatePanel()
        {
            if (statusText == null || lobbyAnnounceToggle == null)
            {
                // Components not loaded yet
                return;
            }

            sessionManager = GameMp.SessionManager;

            if (sessionManager == null || !LobbyConnectionTypePatch.IsPartyMultiplayer)
            {
                statusText.text = "Only supported for custom multiplayer games.";
                statusText.color = Color.yellow;

                lobbyAnnounceToggle.interactable = false;
                lobbyAnnounceToggle.Value = false;
                return;
            }

            if (!LobbyConnectionTypePatch.IsPartyHost)
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
            lobbyAnnounceToggle.interactable = true;
            lobbyAnnounceToggle.Value = LobbyAnnounceToggleValue;

            if (!LobbyAnnounceToggleValue)
            {
                // Currently disabled
                if (LobbyStateManager.DidLeakCurrentCode)
                {
                    statusText.text = "Announcement cancelled";
                    statusText.color = Color.red;
                }
                else
                {
                    statusText.text = "Turn this on to announce your server to the world";
                    statusText.color = Color.yellow;
                }
                
                return;
            }

            // Currently enabled
            statusText.text = LobbyStateManager.StatusText;
            statusText.color = LobbyStateManager.HasErrored ? Color.red : Color.green;
        }
        #endregion
    }
}