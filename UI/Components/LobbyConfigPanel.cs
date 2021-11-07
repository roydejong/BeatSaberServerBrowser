using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.Game.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Components
{
    #pragma warning disable CS0649
    internal class LobbyConfigPanel : NotifiableSingleton<LobbyConfigPanel>
    {
        public const string ResourceName = "ServerBrowser.UI.BSML.LobbyConfigPanel.bsml";
        
        public static readonly Color ColorWarning = new Color(254f / 255f, 202f / 255f, 87f / 255f);
        public static readonly Color ColorError = new Color(238f / 255f, 82f / 255f, 83f / 255f);
        public static readonly Color ColorSuccess = new Color(46f / 255f, 204f / 255f, 113f / 255f);
        
        #region LobbyAnnounceToggle
        [UIComponent("lobbyAnnounceToggle")]
        public ToggleSetting LobbyAnnounceToggle;

        [UIValue("lobbyAnnounceToggle")]
        public bool LobbyAnnounceToggleValue
        {
            get
            {
                if (_activity?.IsQuickPlay ?? false)
                    return Plugin.Config.ShareQuickPlayGames;
                
                return Plugin.Config.LobbyAnnounceToggle;
            }

            set
            {
                if (_activity?.IsQuickPlay ?? false)
                    Plugin.Config.ShareQuickPlayGames = value;
                
                Plugin.Config.LobbyAnnounceToggle = value;
                
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
        
        #region Unity/Mod events
        private MultiplayerActivity? _activity = null;
        
        public override void OnEnable()
        {
            base.OnEnable();
            
            MpEvents.ActivityUpdated += OnActivityUpdated;
        }

        public void OnDisable()
        {
            MpEvents.ActivityUpdated -= OnActivityUpdated;
        }

        private void OnActivityUpdated(object sender, MultiplayerActivity activity)
        {
            _activity = activity;
            UpdatePanel();
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
            if (StatusText is null || LobbyAnnounceToggle is null || NameButton is null)
                // BSML did not yet assign components
                return;

            if (_activity is null || !_activity.IsInMultiplayer)
            {
                // No activity data yet or not connected
                SetPanelDisabledState();
                return;
            }

            var weHaveControl = _activity.WeArePartyLeader;
            var weCanAnnounce = weHaveControl || _activity.IsQuickPlay;
            var lobbyIsAnnounced = _activity.IsAnnounced;
            
            LobbyAnnounceToggle.interactable = weCanAnnounce;
            LobbyAnnounceToggle.Text = _activity.IsQuickPlay
                ? "Share this Quick Play game to the Server Browser"
                : "Add my game to the Server Browser";
            
            NameButton.interactable = weHaveControl && LobbyAnnounceToggleValue;

            if (weCanAnnounce)
            {
                // We are the party leader, or can announce a quick play lobby
                LobbyAnnounceToggle.Value = LobbyAnnounceToggleValue;
                
                if (LobbyAnnounceToggleValue)
                {
                    MpSession.SessionManager.SetLocalPlayerState("lobbyannounce", true);
                
                    StatusText.text = GameStateManager.StatusText;
                    StatusText.color = GameStateManager.HasErrored ? ColorError : ColorSuccess;   
                }
                else
                {
                    MpSession.SessionManager.SetLocalPlayerState("lobbyannounce", false);
                
                    StatusText.text = _activity.IsQuickPlay 
                        ? "Turn me on to publicly share this Quick Play game ↑" 
                        : "Turn me on to list your server in the browser ↑";
                    StatusText.color = ColorWarning;
                }
            }
            else
            {
                // We are a client without control
                if (lobbyIsAnnounced)
                {
                    LobbyAnnounceToggle.Value = true;

                    StatusText.text = "The host has announced this lobby.";
                    StatusText.color = ColorSuccess;
                }
                else
                {
                    LobbyAnnounceToggle.Value = false;

                    StatusText.text = "The host has not announced this lobby.";
                    StatusText.color = ColorWarning;
                }
            }
        }

        private void SetPanelDisabledState()
        {
            StatusText.text = "!Not initialized!";
            StatusText.color = ColorError;

            LobbyAnnounceToggle.interactable = false;
            LobbyAnnounceToggle.Value = false;
            NameButton.interactable = false;
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