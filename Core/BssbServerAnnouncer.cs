using System;
using System.Diagnostics;
using ServerBrowser.Models;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Core
{
    public class BssbServerAnnouncer : MonoBehaviour, IInitializable, IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly BssbDataCollector _dataCollector = null!;

        private bool _sessionEstablished;
        private Stopwatch _announceStopwatch = null!;

        public BssbServerDetail Data => _dataCollector.Current;

        public enum AnnouncerState : byte
        {
            NotAnnouncing,
            AnnouncePending,
            AnnounceSent,
            AnnounceConfirmed
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

            _announceStopwatch = new();
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
        }
        
        public void OnDisable()
        {
            CancelInvoke(nameof(RepeatingTick));
        }

        private void RepeatingTick()
        {
        }
        #endregion

        #region API
        private void SendAnnounceNow()
        {
            // TODO API->Send
        }

        private void SendUnannounceNow()
        {
            // TODO API->Send
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
            
            State = AnnouncerState.AnnouncePending;
            
            if (_dataCollector.Current.LocalPlayer is not null)
                _dataCollector.Current.LocalPlayer.IsAnnouncing = true;

            SendAnnounceNow();
        }

        private void HandleDataChanged(object sender, EventArgs e)
        {
            if (!_sessionEstablished)
                // We never announce until session is fully established
                return;
            
            if (State is not (AnnouncerState.AnnounceSent or AnnouncerState.AnnounceConfirmed))
                // We are NOT already announcing; we can start now if allowed (e.g. after host migration)
                if (!ShouldAnnounce)
                    return;
            
            _log.Info("Enqueued announce (data update)");
            
            State = AnnouncerState.AnnouncePending;
            
            if (_dataCollector.Current.LocalPlayer is not null)
                _dataCollector.Current.LocalPlayer.IsAnnouncing = true;
            
            // TODO Timed/merged updates
        }

        private void HandleDataSessionEnded(object sender, EventArgs e)
        {
            _sessionEstablished = false;
            
            _log.Info("Enqueued unannounce (session ended)");
            
            // TODO Unannounce states
            
            if (_dataCollector.Current.LocalPlayer is not null)
                _dataCollector.Current.LocalPlayer.IsAnnouncing = false;
            
            SendUnannounceNow();
        }
        #endregion
    }
}