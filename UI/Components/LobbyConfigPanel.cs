using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using ServerBrowser.Core;
using ServerBrowser.Harmony;
using ServerBrowser.Utils;
using TMPro;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    internal class LobbyConfigPanel : NotifiableSingleton<LobbyConfigPanel>
    {
        public const string ResourceName = "ServerBrowser.UI.BSML.LobbyConfigPanel.bsml";

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
            GameStateManager.HandleUpdate();
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

            if (sessionManager == null || !MpLobbyConnectionTypePatch.IsPartyMultiplayer)
            {
                statusText.text = "Only supported for custom multiplayer games.";
                statusText.color = Color.yellow;

                lobbyAnnounceToggle.interactable = false;
                lobbyAnnounceToggle.Value = false;
                return;
            }

            if (!MpLobbyConnectionTypePatch.IsPartyHost)
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
                if (GameStateManager.DidLeakCurrentCode)
                {
                    statusText.text = "Cancelled, now removed from browser\r\nNOTE: Your server code may have been seen already";
                    statusText.color = Color.red;
                }
                else
                {
                    statusText.text = "Turn me on to list your server in the browser ↑";
                    statusText.color = Color.yellow;
                }
                
                return;
            }

            // Currently enabled
            statusText.text = GameStateManager.StatusText;
            statusText.color = GameStateManager.HasErrored ? Color.red : Color.green;
        }
        #endregion

        #region BSML Modifier Tab
        private const string TAB_NAME = "Server Browser";

        private static bool _tabIsAdded = false;
        public static void RegisterGameplayModifierTab()
        {
            // TODO one day figure out how to NOT do this in single player,
            //   doesn't currently seem possible to remove it conditionally though

            if (!_tabIsAdded)
            {
                GameplaySetup.instance.AddTab(TAB_NAME, ResourceName, LobbyConfigPanel.instance);
                Plugin.Log?.Debug("Added gameplay modifier tab (LobbyConfigPanel)");
                _tabIsAdded = true;
            }
        }
        #endregion
    }
}