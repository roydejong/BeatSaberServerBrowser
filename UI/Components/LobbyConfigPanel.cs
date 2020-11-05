using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.Harmony;
using ServerBrowser.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Components
{
    internal class LobbyConfigPanel : NotifiableSingleton<LobbyConfigPanel>
    {
        public const string ResourceName = "ServerBrowser.UI.BSML.LobbyConfigPanel.bsml";

        #region LobbyAnnounceToggle
        [UIComponent("lobbyAnnounceToggle")]
        public ToggleSetting LobbyAnnounceToggle;

        [UIValue("lobbyAnnounceToggle")]
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

        [UIAction("lobbyAnnounceToggle")]
        public void SetLobbyAnnounceToggle(bool value)
        {
            LobbyAnnounceToggleValue = value;
            GameStateManager.HandleUpdate();
        }
        #endregion

        #region StatusText
        [UIComponent("statusText")]
        public TextMeshProUGUI StatusText;
        #endregion

        #region Set Game Name
        [UIParams]
        public BSMLParserParams ParserParams;

        [UIComponent("mainContentRoot")]
        public VerticalLayoutGroup MainContentRoot;

        private string _nameValue = "";
        [UIValue("nameValue")]
        public string NameValue
        {
            get => Plugin.Config.CustomGameName;

            set
            {

                Plugin.Config.CustomGameName = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("nameKeyboardSubmit")]
        private async void NameKeyboardSubmit(string text)
        {
            Plugin.Log?.Info($"Set custom game name to: \"{text}\"");

            NameValue = text;

            // Make main content visible again
            MainContentRoot.gameObject.SetActive(true);

            // Trigger update
            GameStateManager.HandleCustomGameName(text);
        }


        [UIComponent("nameButton")]
        public Button NameButton;

        [UIAction("nameButtonClick")]
        private void NameButtonClick()
        {
            ParserParams.EmitEvent("openNameKeyboard");
        }
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
            if (StatusText == null || LobbyAnnounceToggle == null || NameButton == null)
            {
                // Components not loaded yet
                return;
            }

            sessionManager = MpSession.SessionManager;

            if (sessionManager == null || !MpLobbyConnectionTypePatch.IsPartyMultiplayer)
            {
                StatusText.text = "Only supported for custom multiplayer games.";
                StatusText.color = Color.yellow;

                LobbyAnnounceToggle.interactable = false;
                LobbyAnnounceToggle.Value = false;
                NameButton.interactable = false;
                return;
            }

            if (!MpLobbyConnectionTypePatch.IsPartyHost)
            {
                // We are not the host
                LobbyAnnounceToggle.interactable = false;

                var theHost = sessionManager.connectionOwner;

                if (theHost != null && theHost.HasState("lobbyannounce"))
                {
                    LobbyAnnounceToggle.Value = true;

                    StatusText.text = "The host has announced this lobby.";
                    StatusText.color = Color.green;
                }
                else
                {
                    LobbyAnnounceToggle.Value = false;

                    StatusText.text = "The host has not announced this lobby.";
                    StatusText.color = Color.red;
                }

                NameButton.interactable = false;
                return;
            }

            // We are the host, enable controls
            LobbyAnnounceToggle.interactable = true;
            LobbyAnnounceToggle.Value = LobbyAnnounceToggleValue;

            if (!LobbyAnnounceToggleValue)
            {
                // Toggle is disabled, however
                if (GameStateManager.DidLeakCurrentCode)
                {
                    StatusText.text = "Cancelled, now removed from browser\r\nNOTE: Your server code may have been seen already";
                    StatusText.color = Color.red;
                }
                else
                {
                    StatusText.text = "Turn me on to list your server in the browser ↑";
                    StatusText.color = Color.yellow;
                }

                NameButton.interactable = false;
                return;
            }

            // Fallthrough: enabled & all good to go
            StatusText.text = GameStateManager.StatusText;
            StatusText.color = GameStateManager.HasErrored ? Color.red : Color.green;

            NameButton.interactable = true;
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