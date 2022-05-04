using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ModestTree;
using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.Models;
using ServerBrowser.UI.Components;
using ServerBrowser.UI.Utils;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Views
{
    [HotReload]
    public class ServerBrowserDetailViewController : BSMLAutomaticViewController
    {
        public const float RefreshIntervalSecs = 30f;

        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly DiContainer _container = null!;
        [Inject] private readonly ServerBrowserClient _bssbClient = null!;
        [Inject] private readonly BssbApiClient _apiClient = null!;
        [Inject] private readonly CoverArtLoader _coverArtLoader = null!;

        [UIComponent("errorRoot")] private readonly VerticalLayoutGroup _errorRoot = null!;
        [UIComponent("idleRoot")] private readonly VerticalLayoutGroup _idleRoot = null!;
        [UIComponent("loadRoot")] private readonly VerticalLayoutGroup _loadRoot = null!;
        [UIComponent("mainRoot")] private readonly VerticalLayoutGroup _mainRoot = null!;

        [UIComponent("errorText")] private readonly FormattableText _errorText = null!;

        [UIComponent("headerPanelTop")] private readonly VerticalLayoutGroup _headerPanelTop = null!;
        [UIComponent("titleBarRoot")] private readonly VerticalLayoutGroup _titleBarRoot = null!;
        [UIComponent("playerCountText")] private readonly FormattableText _playerCountText = null!;

        [UIComponent("txtServerType")] private readonly FormattableText _txtServerType = null!;
        [UIComponent("txtMasterServer")] private readonly FormattableText _txtMasterServer = null!;
        [UIComponent("txtUptime")] private readonly FormattableText _txtUptime = null!;
        [UIComponent("txtLobbyStatus")] private readonly FormattableText _txtLobbyStatus = null!;
        [UIComponent("txtDifficulty")] private readonly FormattableText _txtDifficulty = null!;
        [UIComponent("txtGameVersion")] private readonly FormattableText _txtGameVersion = null!;
        [UIComponent("txtMpCore")] private readonly FormattableText _txtMpCore = null!;
        [UIComponent("txtMpEx")] private readonly FormattableText _txtMpEx = null!;

        [UIComponent("playerList-scroll")] private readonly BSMLScrollableContainer _playerListScrollable = null!;
        [UIComponent("playerListRoot")] private readonly VerticalLayoutGroup _playerListRoot = null!;
        [UIComponent("playerListEmptyText")] private readonly FormattableText _playerListEmptyText = null!;
        [UIComponent("playerListRowPrefab")] private readonly HorizontalLayoutGroup _playerListRowPrefab = null!;

        [UIComponent("levelHistory-scroll")] private readonly BSMLScrollableContainer _levelHistoryScrollable = null!;
        [UIComponent("levelHistoryRoot")] private readonly VerticalLayoutGroup _levelHistoryRoot = null!;
        [UIComponent("levelHistoryEmptyText")] private readonly FormattableText _levelHistoryEmptyText = null!;

        private BssbLevelBarClone _levelBar = null!;
        private BssbPlayersTable _playersTable = null!;
        private BssbServerDetail? _currentDetail = null!;
        private bool _busyLoading = false;
        private CancellationTokenSource? _loadingCts;

        public event EventHandler<BssbServer>? ConnectClickedEvent;

        #region Lifecycle events

        [UIAction("#post-parse")]
        private void PostParse()
        {
            // Set correct colors for header panel background
            var headerPanelTopBg = _headerPanelTop.GetComponent<ImageView>();
            headerPanelTopBg.color = new Color(1, 1, 1, .2f);
            headerPanelTopBg.color0 = Color.white;
            headerPanelTopBg.color1 = new Color(1, 1, 1, 0);

            // Create BssbLevelBarClone for title
            _levelBar = BssbLevelBarClone.Create(_container, _titleBarRoot.transform);
            _levelBar.SetImageVisible(true);
            _levelBar.SetBackgroundStyle(BssbLevelBarClone.BackgroundStyle.ColorfulGradient, enableRaycast: true);
            _levelBar.SetText("!Server name!", "!Server type!");

            // Create players table with prefab
            _playersTable = new BssbPlayersTable(_playerListRoot.gameObject, _playerListRowPrefab.gameObject);

            if (_currentDetail is null)
                ClearData();
            else
                SetData(_currentDetail);
        }

        public void OnEnable()
        {
            InvokeRepeating(nameof(RefreshTick), RefreshIntervalSecs, RefreshIntervalSecs);
        }

        public void OnDisable()
        {
            CancelInvoke(nameof(RefreshTick));
        }

        private async void RefreshTick()
        {
            if (_busyLoading)
                return;

            await Refresh(true);
        }

        public async Task Refresh(bool soft = false)
        {
            if (_currentDetail?.Key == null)
                return;

            await LoadDetailsAsync(_currentDetail.Key, soft);
        }

        #endregion

        #region State/data

        public void ShowError(string errorMessage)
        {
            _errorText.SetText(errorMessage);

            _errorRoot.gameObject.SetActive(true);
            _idleRoot.gameObject.SetActive(false);
            _loadRoot.gameObject.SetActive(false);
            _mainRoot.gameObject.SetActive(false);

            _currentDetail = null;
        }

        public void ClearData()
        {
            _errorRoot.gameObject.SetActive(false);
            _idleRoot.gameObject.SetActive(true);
            _loadRoot.gameObject.SetActive(false);
            _mainRoot.gameObject.SetActive(false);

            _currentDetail = null;
        }

        public async Task LoadDetailsAsync(string serverKey, bool soft = false)
        {
            CancelLoading();

            _busyLoading = true;

            try
            {
                if (!soft)
                {
                    _errorRoot.gameObject.SetActive(false);
                    _idleRoot.gameObject.SetActive(false);
                    _loadRoot.gameObject.SetActive(true);
                    _mainRoot.gameObject.SetActive(false);
                }

                var serverDetail = await _apiClient.BrowseDetail(serverKey, _loadingCts!.Token);

                if (serverDetail == null)
                {
                    ShowError("Failed to load server details");
                    return;
                }

                SetData(serverDetail);
            }
            finally
            {
                _busyLoading = false;
            }
        }

        public void CancelLoading()
        {
            _loadingCts?.Cancel();
            _loadingCts?.Dispose();

            _loadingCts = new();
        }

        public void SetData(BssbServerDetail serverDetail)
        {
            try
            {
                _currentDetail = serverDetail;

                _errorRoot.gameObject.SetActive(false);
                _idleRoot.gameObject.SetActive(false);
                _loadRoot.gameObject.SetActive(false);
                _mainRoot.gameObject.SetActive(true);
                
                SetHeaderData(serverDetail);
                SetInfoTabData(serverDetail);
                SetPlayerData(serverDetail.Players);
                SetLevelHistoryData(serverDetail.LevelHistory);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                ShowError("Error while presenting data");
            }
        }

        private void SetHeaderData(BssbServerDetail serverDetail)
        {
            _levelBar.SetText(serverDetail.Name, serverDetail.LobbyStateTextExtended);

            // Player count
            var playerCount = serverDetail.PlayerCount;
            var playerLimit = serverDetail.PlayerLimit ?? 5;
            _playerCountText.SetText($"{playerCount}/{playerLimit}");
            _playerCountText.color = (playerCount < playerLimit ? Color.white : BssbColorScheme.Red);

            // Cover art 
            if (serverDetail.IsInLobby)
            {
                // Not in game, show lobby icon
                _levelBar.SetImageSprite(Sprites.PortalUser);
            }
            else
            {
                // In game, show cover art
                _levelBar.SetImageSprite(Sprites.BeatSaverLogo);

                _coverArtLoader.FetchCoverArtAsync(new CoverArtLoader.CoverArtRequest(serverDetail, _loadingCts!.Token,
                    sprite =>
                    {
                        if (sprite != null)
                            _levelBar.SetImageSprite(sprite);
                    }));
            }
        }

        private void SetInfoTabData(BssbServerDetail serverDetail)
        {
            _txtServerType.SetText(serverDetail.ServerTypeText ?? "Unknown");
            _txtMasterServer.SetText(serverDetail.MasterServerText ??
                                     serverDetail.MasterServerEndPoint?.hostName ?? "Unknown");
            _txtUptime.SetText(serverDetail.LobbyLifetimeText);
            _txtLobbyStatus.SetText(serverDetail.LobbyStateText);

            if (!serverDetail.IsQuickPlay && serverDetail.Difficulty != null)
                _txtDifficulty.SetText(serverDetail.DifficultyNameWithColor);
            else
                _txtDifficulty.SetText("New lobby");

            _txtGameVersion.SetText($"Beat Saber {serverDetail.GameVersion}");
            _txtGameVersion.color = (serverDetail.GameVersion?.Equals(IPA.Utilities.UnityGame.GameVersion) ?? false)
                ? BssbColorScheme.Green
                : BssbColorScheme.Gold;

            if (serverDetail.MultiplayerCoreVersion is not null)
            {
                _txtMpCore.SetText($"Core v{serverDetail.MultiplayerCoreVersion}");
                _txtMpCore.color = _bssbClient.MultiplayerCoreVersion == serverDetail.MultiplayerCoreVersion
                    ? BssbColorScheme.Green
                    : BssbColorScheme.Red;
            }
            else
            {
                _txtMpCore.SetText($"None");
                _txtMpCore.color = BssbColorScheme.MutedGray;
            }

            if (serverDetail.MultiplayerExtensionsVersion is not null)
            {
                _txtMpEx.SetText($"Extensions v{serverDetail.MultiplayerExtensionsVersion}");
                _txtMpEx.color = _bssbClient.MultiplayerExtensionsVersion == serverDetail.MultiplayerExtensionsVersion
                    ? BssbColorScheme.Green
                    : BssbColorScheme.Red;
            }
            else
            {
                _txtMpEx.SetText($"None");
                _txtMpEx.color = BssbColorScheme.MutedGray;
            }
        }

        private void SetPlayerData(IReadOnlyCollection<BssbServerPlayer> players)
        {
            _playerListEmptyText.gameObject.SetActive(!players.Any());

            _playersTable.SetData(players);

            ResetPlayerListScroll();
        }

        private void ResetPlayerListScroll()
        {
            _playerListScrollable.ContentSizeUpdated();
            _playerListScrollable.ScrollTo(0, false);
        }

        private void SetLevelHistoryData(IReadOnlyCollection<BssbServerLevel> levelHistory)
        {
            // Clear previous entries
            foreach (var childElement in _levelHistoryRoot.GetComponentsInChildren<BssbLevelBarClone>())
                if (childElement != null)
                    Destroy(childElement.gameObject);

            // Show "empty" text if needed
            if (levelHistory.IsEmpty())
            {
                _levelHistoryEmptyText.gameObject.SetActive(true);
                return;
            }

            _levelHistoryEmptyText.gameObject.SetActive(false);

            // Insert level bars
            foreach (var historyItem in levelHistory)
            {
                var levelBar = BssbLevelBarClone.Create(_container, _levelHistoryRoot.transform, true);
                levelBar.SetText(historyItem.SongName ?? "A song", historyItem.SongAuthorName ?? historyItem.LevelAuthorName ?? historyItem.SessionGameId ?? "pls bro");
                levelBar.SetBackgroundStyle(BssbLevelBarClone.BackgroundStyle.GameDefault);
                
                levelBar.SetImageSprite(Sprites.BeatSaverLogo);
                _coverArtLoader.FetchCoverArtAsync(new CoverArtLoader.CoverArtRequest(historyItem, _loadingCts!.Token,
                    sprite =>
                    {
                        if (sprite != null)
                            levelBar.SetImageSprite(sprite);
                    }));
            }
            
            ResetLevelHistoryScroll();
        }

        private void ResetLevelHistoryScroll()
        {
            _levelHistoryScrollable.ContentSizeUpdated();
            _levelHistoryScrollable.ScrollTo(0, false);
        }

        #endregion

        #region UI Events

        [UIAction("connectButtonClick")]
        private void HandleConnectButtonClick()
        {
            if (_currentDetail is null)
                return;

            ConnectClickedEvent?.Invoke(this, _currentDetail);
        }

        #endregion
    }
}