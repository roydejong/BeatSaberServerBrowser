using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Util;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Models;
using ServerBrowser.UI.Components;
using ServerBrowser.UI.Utils;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Lobby
{
    [HotReload]
    public class LobbyConfigPanel : NotifiableSingleton<LobbyConfigPanel>, IInitializable, IDisposable
    {
        private const string ResourceName = "ServerBrowser.UI.Lobby.LobbyConfigPanel.bsml";
        private const string TabName = "Server Browser";
        
        [Inject] private readonly GameplaySetup _gameplaySetup = null!;

        [Inject] private readonly BssbDataCollector _dataCollector = null!;
        [Inject] private readonly BssbServerAnnouncer _serverAnnouncer = null!;
        [Inject] private readonly BssbFloatingAlert _floatingAlert = null!;
        [Inject] private readonly ServerBrowserClient _serverBrowserClient = null!;

        #region Events 

        public void Initialize()
        {
            _gameplaySetup.AddTab(
                name: TabName,
                resource: ResourceName,
                host: this,
                menuType: MenuType.Online
            );
            
            _dataCollector.DataChanged += (sender, args) => Refresh();

            _serverAnnouncer.OnAnnounceResult += (sender, response) => Refresh();
            _serverAnnouncer.OnUnAnnounceResult += (sender, success) => Refresh();
            _serverAnnouncer.OnStateChange += (sender, state) => Refresh();
        }
        
        public void Dispose()
        {
            _gameplaySetup.RemoveTab(TabName);
        }
        
        #endregion

        #region BSML Instance

        [UIParams] private readonly BSMLParserParams _parserParams = null!;

        [UIComponent("mainContentRoot")] private readonly VerticalLayoutGroup _mainContentRoot = null!;
        [UIComponent("lobbyAnnounceToggle")] private readonly ToggleSetting _toggleAnnounce = null!;
        [UIComponent("nameButton")] private readonly Button _buttonServerName = null!;
        [UIComponent("statusText")] private readonly TextMeshProUGUI _labelStatus = null!;

        [UIComponent("joinNotificationsEnabled")]
        private readonly ToggleSetting _toggleNotifications = null!;

        [UIValue("lobbyAnnounceToggle")]
        public bool LobbyAnnounceToggleValue
        {
            get => IsQuickPlay ? Plugin.Config.AnnounceQuickPlay : Plugin.Config.AnnounceParty;

            set
            {
                if (IsQuickPlay)
                    Plugin.Config.AnnounceQuickPlay = value;
                else
                    Plugin.Config.AnnounceParty = value;

                NotifyPropertyChanged();
                _serverAnnouncer.RefreshPreferences();
                Refresh();
            }
        }

        [UIValue("joinNotificationsEnabled")]
        public bool JoinNotificationsEnabledValue
        {
            get => Plugin.Config.EnableJoinNotifications;

            set
            {
                Plugin.Config.EnableJoinNotifications = value;

                NotifyPropertyChanged();

                if (value)
                {
                    _floatingAlert.PresentNotification(new BssbFloatingAlert.NotificationData
                    (
                        Sprites.AnnouncePadded,
                        "Join notifications enabled",
                        "When players join or leave the lobby, you'll be notified.",
                        BssbLevelBarClone.BackgroundStyle.SolidBlue,
                        false
                    ));
                }
                else
                {
                    _floatingAlert.DismissAnimated();
                }
            }
        }

        [UIValue("nameValue")]
        public string NameValue
        {
            get => _serverBrowserClient.PreferredServerName;

            set
            {
                Plugin.Config.ServerName = value;

                NotifyPropertyChanged();
                _serverAnnouncer.RefreshPreferences();
                Refresh();
            }
        }

        [UIAction("nameKeyboardSubmit")]
        private void HandleNameKeyboardSubmit(string text)
        {
            _mainContentRoot.gameObject.SetActive(true);
            NameValue = text;
        }

        [UIAction("nameButtonClick")]
        private void HandleNameButtonClick()
        {
            _parserParams.EmitEvent("openNameKeyboard");
        }

        #endregion

        #region Logic

        private BssbServerDetail? SessionInfo => _dataCollector.Current;
        public bool SessionIsActive => _dataCollector.SessionActive && SessionInfo is not null;
        private bool IsPartyLeader => SessionInfo?.LocalPlayer?.IsPartyLeader ?? false;
        private bool IsQuickPlay => SessionInfo?.IsQuickPlay ?? false;

        public void Refresh()
        {
            _toggleAnnounce.ReceiveValue();
            _toggleNotifications.ReceiveValue();

            _toggleNotifications.Interactable = true;

            if (!SessionIsActive)
            {
                _labelStatus.text = "Session not connected";
                _labelStatus.color = BssbColorScheme.MutedGray;

                _toggleAnnounce.Interactable = false;
                _buttonServerName.interactable = false;
                return;
            }

            if ((SessionInfo?.IsDirectConnect ?? false) || (SessionInfo?.IsBeatNetHost ?? false))
            {
                _labelStatus.text = "This server controls its own announcements";
                _labelStatus.color = BssbColorScheme.Gold;

                _toggleAnnounce.Interactable = false;
                _buttonServerName.interactable = false;
                return;
            }

            if (!IsQuickPlay && !IsPartyLeader)
            {
                _labelStatus.text = "You are not the party leader";
                _labelStatus.color = BssbColorScheme.MutedGray;

                _toggleAnnounce.Interactable = false;
                _buttonServerName.interactable = false;
                return;
            }

            // Have control
            _toggleAnnounce.Interactable = true;

            switch (_serverAnnouncer.State)
            {
                case BssbServerAnnouncer.AnnouncerState.Announcing:
                {
                    if (_serverAnnouncer.AnnounceServerMessage != null)
                    {
                        _labelStatus.text = $"{_serverAnnouncer.AnnounceServerMessage}";
                        _labelStatus.color = BssbColorScheme.Red;
                    }
                    else if (_serverAnnouncer.HaveAnnounceSuccess)
                    {
                        _labelStatus.text =
                            $"Players can join from the Server Browser!\r\n{_serverAnnouncer.Data.Name}";
                        _labelStatus.color = BssbColorScheme.Green;
                    }
                    else
                    {
                        _labelStatus.text = $"Announcing to Server Browser...\r\n{_serverAnnouncer.Data.Name}";
                        _labelStatus.color = BssbColorScheme.Gold;
                    }

                    _buttonServerName.interactable = !_serverAnnouncer.Data.IsQuickPlay;
                    break;
                }
                case BssbServerAnnouncer.AnnouncerState.Unannouncing:
                {
                    _labelStatus.text = "Removing from Server Browser...";
                    _labelStatus.color = BssbColorScheme.Gold;

                    _buttonServerName.interactable = false;
                    break;
                }
                default:
                case BssbServerAnnouncer.AnnouncerState.NotAnnouncing:
                {
                    _labelStatus.text = "Not announcing\r\nFlip the switch to add your server to the browser ↑";
                    _labelStatus.color = BssbColorScheme.Red;

                    _buttonServerName.interactable = false;
                    break;
                }
            }
        }

        #endregion
    }
}