﻿using BeatSaberMarkupLanguage.Attributes;
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
    #pragma warning disable CS0649
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
                if (MpLobbyConnectionTypePatch.IsPartyHost)
                    return Plugin.Config.LobbyAnnounceToggle;
                if (MpLobbyConnectionTypePatch.IsQuickplay)
                    return Plugin.Config.ShareQuickPlayGames;
                return false;
            }

            set
            {
                if (MpLobbyConnectionTypePatch.IsPartyHost)
                    Plugin.Config.LobbyAnnounceToggle = value;
                if (MpLobbyConnectionTypePatch.IsQuickplay)
                    Plugin.Config.ShareQuickPlayGames = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("lobbyAnnounceToggle")]
        public void SetLobbyAnnounceToggle(bool value)
        {
            LobbyAnnounceToggleValue = value;
            GameStateManager.HandleUpdate(false);
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
        private void NameKeyboardSubmit(string text)
        {
            // Make main content visible again
            MainContentRoot.gameObject.SetActive(true);

            // Name update on UI and in settings
            NameValue = text;
            text = MpSession.GetHostGameName(); // this will read CustomGameName but fall back to a default name if left empty
            NameValue = text;

            // Announce update
            GameStateManager.HandleUpdate(true);
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

            if (sessionManager == null ||
                (!MpLobbyConnectionTypePatch.IsPartyMultiplayer && !MpLobbyConnectionTypePatch.IsQuickplay))
            {
                StatusText.text = "Only supported for custom and Quick Play games.";
                StatusText.color = Color.yellow;

                LobbyAnnounceToggle.interactable = false;
                LobbyAnnounceToggle.Value = false;
                NameButton.interactable = false;
                return;
            }

            if (MpLobbyConnectionTypePatch.IsPartyMultiplayer && !MpLobbyConnectionTypePatch.IsPartyHost)
            {
                // Party but we are not the host
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
            
            if (MpLobbyConnectionTypePatch.IsQuickplay)
                LobbyAnnounceToggle.Text = "Share this Quick Play game to the Server Browser";
            else
                LobbyAnnounceToggle.Text = "Add my game to the Server Browser";

            if (!LobbyAnnounceToggleValue)
            {
                // Toggle is disabled, however
                if (MpLobbyConnectionTypePatch.IsQuickplay)
                    StatusText.text = "Turn me on to publicly share this Quick Play game ↑";
                else
                    StatusText.text = "Turn me on to list your server in the browser ↑";
                
                StatusText.color = Color.yellow;

                NameButton.interactable = false;
                return;
            }

            // Fallthrough: enabled & all good to go
            StatusText.text = GameStateManager.StatusText;
            StatusText.color = GameStateManager.HasErrored ? Color.red : Color.green;

            NameButton.interactable = MpLobbyConnectionTypePatch.IsPartyHost;
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
    #pragma warning restore CS0649  
}