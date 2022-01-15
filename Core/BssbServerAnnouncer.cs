using System;
using System.Threading.Tasks;
using ServerBrowser.Models;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Core
{
    public class BssbServerAnnouncer : MonoBehaviour, IInitializable, IDisposable
    {
        public const float AnnounceIntervalSeconds = 3f;
        
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly BssbDataCollector _dataCollector = null!;
        [Inject] private readonly BssbApiClient _apiClient = null!;

        private bool _sessionEstablished;
        private bool _dirtyFlag;
        private bool _havePendingRequest;
        private float? _lastAnnounceTime;

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

        #region Zenject lifecycle

        public void Initialize()
        {
            _dataCollector.SessionEstablished += HandleDataSessionEstablished;
            _dataCollector.DataChanged += HandleDataChanged;
            _dataCollector.SessionEnded += HandleDataSessionEnded;
        }

        public void Dispose()
        {
            _dataCollector.SessionEstablished -= HandleDataSessionEstablished;
            _dataCollector.DataChanged -= HandleDataChanged;
            _dataCollector.SessionEnded -= HandleDataSessionEnded;
        }

        #endregion

        #region Unity events

        public void OnEnable()
        {
            InvokeRepeating(nameof(RepeatingTick), 1f, 1f);

            _sessionEstablished = false;
            _dirtyFlag = true;
            _havePendingRequest = false;
        }

        public void OnDisable()
        {
            CancelInvoke(nameof(RepeatingTick));
        }

        private async Task RepeatingTick()
        {
            if (_havePendingRequest)
                // Currently awaiting (un)announce request, do nothing
                return;

            if (!_dirtyFlag)
            {
                var timeNow = Time.realtimeSinceStartup;

                if (_lastAnnounceTime == null || (_lastAnnounceTime - timeNow) >= AnnounceIntervalSeconds)
                {
                    // Interval time reached, mark update needed
                    _dirtyFlag = true;
                    _lastAnnounceTime = timeNow;
                }
                else
                {
                    // No update to send, and interval time has not yet been reached
                    return;
                }
            }
            
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
            }
        }

        #endregion

        #region API

        private async Task<bool> SendAnnounceNow()
        {
            _dirtyFlag = false;
            _havePendingRequest = true;
            
            var response = await _apiClient.Announce(_dataCollector.Current);
            
            _havePendingRequest = false;

            if (response?.Success ?? false)
            {
                _log.Info("Announce OK");
                return true;
            }

            var failReason = "unknown";

            if (response is null)
                failReason = "No response";
            else if (!response.Success)
                failReason = "Error response";
            
            _log.Error($"Announce failed: {failReason}");
            _dirtyFlag = true;
            return false;
        }

        private async Task<bool> SendUnannounceNow()
        {
            _dirtyFlag = false;
            _havePendingRequest = true;
            
            var response = await _apiClient.UnAnnounce();
            
            _havePendingRequest = false;

            if (response?.IsOk ?? false)
            {
                _log.Info("Unannounce OK");
                return true;
            }
            
            _log.Error("Announce failed");
            _dirtyFlag = true;
            return false;
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
        }

        private void HandleDataChanged(object sender, EventArgs e)
        {
            if (!_sessionEstablished)
                // We never announce until session is fully established
                return;

            if (State is not AnnouncerState.Announcing)
                // We are NOT already announcing; we can start now if allowed (e.g. after host migration)
                if (!ShouldAnnounce)
                    return;

            State = AnnouncerState.Announcing;

            _dirtyFlag = true;
        }

        private void HandleDataSessionEnded(object sender, EventArgs e)
        {
            _sessionEstablished = false;

            State = AnnouncerState.Unannouncing;

            _dirtyFlag = true;

            _ = SendUnannounceNow();
        }

        #endregion
    }
}