using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.Harmony;
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
            // Make main content visible again
            MainContentRoot.gameObject.SetActive(true);

            // Name update on UI and in settings
            NameValue = text;
            text = MpSession.GetHostGameName(); // this will read CustomGameName but fall back to a default name if left empty
            NameValue = text;

            // Name update on announce
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

        #region JoinNotificationsEnabled
        [UIComponent("joinNotificationsEnabled")]
        public ToggleSetting JoinNotificationsEnabled;

        [UIValue("joinNotificationsEnabled")]
        public bool JoinNotificationsEnabledValue
        {
            get
            {
                return Plugin.Config.JoinNotificationsEnabled;
            }

            set
            {
                Plugin.Config.JoinNotificationsEnabled = value;
            }
        }

        [UIAction("joinNotificationsEnabled")]
        public void SetJoinNotificationsEnabled(bool value)
        {
            JoinNotificationsEnabledValue = value;

            if (value)
            {
                FloatingNotification.Instance.ShowMessage(
                    "Notifications enabled",
                    "You'll be notified if players join or leave",
                    FloatingNotification.NotificationStyle.Blue,
                    Sprites.Portal
                );
            }
            else
            {
                FloatingNotification.Instance.DismissMessage();
            }
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
            if (!_tabIsAdded)
            {
                GameplaySetup.instance.AddTab(TAB_NAME, ResourceName, LobbyConfigPanel.instance, MenuType.Online);
                Plugin.Log?.Debug("Added gameplay modifier tab (LobbyConfigPanel)");
                _tabIsAdded = true;
            }
        }
        #endregion
    }
}