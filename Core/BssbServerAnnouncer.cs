using System;
using System.Threading.Tasks;
using ServerBrowser.Models;
using ServerBrowser.Models.Requests;
using ServerBrowser.Models.Responses;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Handles sending announce/unannounce requests to the server browser API. 
    /// </summary>
    public class BssbServerAnnouncer : MonoBehaviour, IInitializable
    {
        private const float AnnounceIntervalSeconds = 30f;
        private const int MaxConsecutiveErrorsForUnannounce = 3; 
        
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly BssbDataCollector _dataCollector = null!;
        [Inject] private readonly BssbApiClient _apiClient = null!;
        [Inject] private readonly ServerBrowserClient _serverBrowserClient = null!;

        private bool _sessionEstablished;
        private bool _dirtyFlag;
        private bool _havePendingRequest;
        private float? _lastAnnounceTime;
        private AnnounceResponse? _lastSuccessResponse;
        private int _consecutiveErrors = 0;
        private string? _lastResultsSessionId;

        public BssbServerDetail Data => _dataCollector.Current;

        public enum AnnouncerState : byte
        {
            /// <summary>
            /// Announcing has not yet begun, or unannounce has completed.
            /// May transition to Announcing if the session starts, and we are allowed to announce.
            /// </summary>
            NotAnnouncing,

            /// <summary>
            /// Session was established, and we are now actively sending periodic announces.
            /// </summary>
            Announcing,

            /// <summary>
            /// Session has ended, and we are unannouncing the game.
            /// Will transition to NotAnnouncing once the unannounce is confirmed.
            /// </summary>
            Unannouncing
        }

        private AnnouncerState _stateBackingField = AnnouncerState.NotAnnouncing;

        public AnnouncerState State
        {
            get => _stateBackingField;
            set
            {
                if (_stateBackingField == value)
                    return;
                
                _stateBackingField = value;
                _lastSuccessResponse = null;
                _dirtyFlag = true;
                
                OnStateChange?.Invoke(this, value);
            }
        }

        public bool HaveAnnounceSuccess => State == AnnouncerState.Announcing && _lastSuccessResponse is not null &&
                                           _lastSuccessResponse.Success;

        public string? AnnounceServerMessage => State == AnnouncerState.Announcing
            ? _lastSuccessResponse?.ServerMessage : null;
        
        public event EventHandler<AnnounceResponse?>? OnAnnounceResult;
        public event EventHandler<bool>? OnUnAnnounceResult;
        public event EventHandler<AnnouncerState>? OnStateChange;

        #region Lifecycle

        public void Initialize()
        {
            _dataCollector.SessionEstablished += HandleDataSessionEstablished;
            _dataCollector.DataChanged += HandleDataChanged;
            _dataCollector.SessionEnded += HandleDataSessionEnded;
        }

        public void OnEnable()
        {
            InvokeRepeating(nameof(RepeatingTick), 3f, 3f);

            _sessionEstablished = false;
            _dirtyFlag = true;
            _havePendingRequest = false;
            _lastAnnounceTime = null;
            _lastSuccessResponse = null;
            _consecutiveErrors = 0;
        }

        public void OnDisable()
        {
            CancelInvoke(nameof(RepeatingTick));
        }
        
        #endregion

        #region Logic

        private bool GetShouldAnnounce()
        {
            if (Data.IsDirectConnect || Data.IsBeatDediHost)
                // Direct connect hosts handle their own announcements and may be intentionally private
                return false;
            
            if (Data.IsQuickPlay)
                return _config.AnnounceQuickPlay; // Quick Play: All players should announce, if not disabled

            return _config.AnnounceParty && _dataCollector.IsPartyLeader;
        }

        public async void RefreshPreferences()
        {
            var isAnnouncing = State is AnnouncerState.Announcing;
            var shouldAnnounce = GetShouldAnnounce();

            if (_sessionEstablished)
            {
                if (shouldAnnounce && !isAnnouncing)
                {
                    _log.Info($"User preferences updated: starting announce");
                    State = AnnouncerState.Announcing;
                }
                else if (!shouldAnnounce && isAnnouncing)
                {
                    _log.Info($"User preferences updated: starting unannounce");
                    State = AnnouncerState.Unannouncing;
                }

                var prefName = _serverBrowserClient.PreferredServerName;
                    
                if ((Data.LocalPlayer?.IsPartyLeader ?? false) && Data.Name != prefName)
                {
                    _log.Info($"User preferences updated: set game name to {prefName}");
                    Data.Name = prefName;
                }
            }

            await RepeatingTick();
        }

        private async Task RepeatingTick()
        {
            if (State == AnnouncerState.NotAnnouncing)
                // Not enabled
                return;
            
            if (_havePendingRequest)
                // Currently awaiting (un)announce request, do nothing
                return;

            if (_dirtyFlag)
            {
                // Have dirty flag, we should send next message now
                if (State == AnnouncerState.Announcing)
                {
                    await SendAnnounceNow();
                }
                else if (State == AnnouncerState.Unannouncing)
                {
                    if (await SendUnannounceNow())
                    {
                        State = AnnouncerState.NotAnnouncing;
                    }
                    else if (_consecutiveErrors >= MaxConsecutiveErrorsForUnannounce)
                    {
                        _log.Warn($"Unannounce aborted: {_consecutiveErrors} consecutive errors");
                        State = AnnouncerState.NotAnnouncing;
                    }
                }
            }
            else
            {
                // Do not have dirty flag, check timing
                var timeNow = Time.realtimeSinceStartup;

                if (_lastAnnounceTime == null || (timeNow - _lastAnnounceTime) >= AnnounceIntervalSeconds)
                {
                    // Interval time reached, mark update needed
                    _dirtyFlag = true;
                }
            }
        }

        #endregion

        #region API

        private async Task<bool> SendAnnounceNow()
        {
            if (Data.LobbyState == MultiplayerLobbyState.None)
                // Not in a lobby (anymore); do not announce
                return false;
            
            try
            {
                _dirtyFlag = false;
                _havePendingRequest = true;

                var response = await _apiClient.Announce(_dataCollector.Current);

                if (response?.Success ?? false)
                {
                    _consecutiveErrors = 0;
                    _log.Info($"Announce OK (ServerKey={response.Key}, ServerMessage={response.ServerMessage})");
                    
                    _lastAnnounceTime = Time.realtimeSinceStartup;
                    _lastSuccessResponse = response;
                    _dataCollector.Current.Key = response.Key;
                    
                    await SendAnnounceResultsIfNeededNow();
                    
                    OnAnnounceResult?.Invoke(this, response);
                    return true;
                }
                else
                {
                    _consecutiveErrors++;
                    _log.Warn($"Announce failed (ServerMessage={response?.ServerMessage})");
                    
                    _dirtyFlag = true;
                    
                    OnAnnounceResult?.Invoke(this, null);
                    return false;
                }
            }
            finally
            {
                _havePendingRequest = false;
            }
        }
        
        private async Task<bool> SendAnnounceResultsIfNeededNow()
        {
            if (_dataCollector.LastResults is null)
                // No results available to announce
                return false;
            
            if (string.IsNullOrEmpty(_dataCollector.LastResults.gameId))
                // Gameplay Session ID (GUID) sometimes doesn't get set, which means it cannot be announced
                return false;

            if (_lastResultsSessionId == _dataCollector.LastResults.gameId)
                // These results were already successfully announced
                return false;
            
            try
            {
                var resultsData = AnnounceResultsData.FromMultiplayerResultsData(_dataCollector.LastResults);
                var responseOk = await _apiClient.AnnounceResults(resultsData);

                if (responseOk)
                {
                    _log.Info($"Level results announced OK (SessionGameId={resultsData.SessionGameId})");
                    _lastResultsSessionId = resultsData.SessionGameId;
                    return true;
                }
                else
                {
                    _log.Warn($"Level results announce failed");
                    return false;
                }
            }
            finally
            {
                _havePendingRequest = false;
            }
        }

        private async Task<bool> SendUnannounceNow()
        {
            var unAnnounceRequest = new UnAnnounceParams()
            {
                SelfUserId = _dataCollector.Current?.LocalPlayer?.UserId,
                HostSecret = _dataCollector.Current?.HostSecret,
                HostUserId = _dataCollector.Current?.RemoteUserId,
            };
            
            if (!unAnnounceRequest.IsComplete)
                return false;
            
            try
            {
                _dirtyFlag = false;
                _havePendingRequest = true;

                var response = await _apiClient.UnAnnounce(unAnnounceRequest);

                if (response?.IsOk ?? false)
                {
                    _consecutiveErrors = 0;
                    _log.Info("Unannounce OK");
                    
                    OnUnAnnounceResult?.Invoke(this, true);
                    return true;
                }
                else
                {
                    _consecutiveErrors++;
                    _log.Warn("Unannounce failed");

                    if (response is null || response.CanRetry)
                    {
                        _dirtyFlag = true;
                    }
                    
                    OnUnAnnounceResult?.Invoke(this, false);
                    return false;
                }
            }
            finally
            {
                _havePendingRequest = false;
            }
        }

        #endregion

        #region Data collector events

        private void HandleDataSessionEstablished(object sender, BssbServerDetail e)
        {
            // Data established for a new session; begin announcing if appropriate for lobby and config
            _sessionEstablished = true;

            if (!GetShouldAnnounce())
                return;

            _log.Info("Starting announcing (session established)");
            State = AnnouncerState.Announcing;
        }

        private void HandleDataChanged(object sender, EventArgs e)
        {
            if (!_sessionEstablished)
                // We never announce until session is fully established
                return;

            if (State != AnnouncerState.Announcing)
            {
                // We are NOT already announcing; we can start now if allowed (e.g. after host migration)
                if (!GetShouldAnnounce())
                    return;
                
                _log.Info("Starting announcing (late/host migration)");
                State = AnnouncerState.Announcing;
            }

            _dirtyFlag = true;
        }

        private async void HandleDataSessionEnded(object sender, EventArgs e)
        {
            _sessionEstablished = false;

            if (State != AnnouncerState.Announcing)
                // We are not announcing
                return;
            
            if (!HaveAnnounceSuccess)
            {
                // We do not have a successful announce to cancel
                State = AnnouncerState.NotAnnouncing;
                return;
            }

            State = AnnouncerState.Unannouncing;
            await SendUnannounceNow();
        }

        #endregion
    }
}