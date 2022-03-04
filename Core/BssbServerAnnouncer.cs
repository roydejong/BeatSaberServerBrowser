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

        public AnnouncerState State { get; private set; } = AnnouncerState.NotAnnouncing;

        public bool ShouldAnnounce
        {
            get
            {
                if (Data.IsQuickPlay)
                    return _config.AnnounceQuickPlay; // Quick Play: All players should announce, if not disabled

                return _config.AnnounceParty && _dataCollector.IsPartyLeader;
            }
        }

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
            try
            {
                _dirtyFlag = false;
                _havePendingRequest = true;

                var response = await _apiClient.Announce(_dataCollector.Current);

                if (response?.Success ?? false)
                {
                    _consecutiveErrors = 0;
                    _log.Info($"Announce OK (ServerKey={response.Key})");
                    _lastAnnounceTime = Time.realtimeSinceStartup;
                    _lastSuccessResponse = response;
                    _dataCollector.Current.Key = response.Key;
                    
                    await SendAnnounceResultsIfNeededNow();
                    
                    return true;
                }
                else
                {
                    _consecutiveErrors++;
                    _log.Warn($"Announce failed");
                    _dirtyFlag = true;
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

            if (!ShouldAnnounce)
                return;

            _log.Info("Starting announcing (session established)");
            State = AnnouncerState.Announcing;
            _dirtyFlag = true;
            _lastSuccessResponse = null;
        }

        private void HandleDataChanged(object sender, EventArgs e)
        {
            if (!_sessionEstablished)
                // We never announce until session is fully established
                return;

            if (State != AnnouncerState.Announcing)
            {
                // We are NOT already announcing; we can start now if allowed (e.g. after host migration)
                if (!ShouldAnnounce)
                    return;
                
                _log.Info("Starting announcing (late/host migration)");
                State = AnnouncerState.Announcing;
                _lastSuccessResponse = null;
            }

            _dirtyFlag = true;
        }

        private async void HandleDataSessionEnded(object sender, EventArgs e)
        {
            _sessionEstablished = false;

            if (State != AnnouncerState.Announcing)
                // We are not announcing
                return;
            
            State = AnnouncerState.Unannouncing;
            
            if (_lastSuccessResponse == null)
                // We do not have a successful announce to cancel
                return;
            
            _dirtyFlag = true;
            await SendUnannounceNow();
        }

        #endregion
    }
}