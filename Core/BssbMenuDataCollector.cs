using System;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Extends the BssbDataCollector with data that is only available in menu scope.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbMenuDataCollector : IInitializable, IDisposable
    {
        [Inject] private SiraLog _log = null!;
        [Inject] private BssbDataCollector _dataCollector = null!;
        [Inject] private ILobbyGameStateController _lobbyGameStateController = null!;
        
        public void Initialize()
        {
            _lobbyGameStateController.lobbyStateChangedEvent += HandleLobbyStateChanged;
        }

        public void Dispose()
        {
            _lobbyGameStateController.lobbyStateChangedEvent -= HandleLobbyStateChanged;
        }
        
        private void HandleLobbyStateChanged(MultiplayerLobbyState newState)
        {
            if (_dataCollector.Current.LobbyState == newState)
                return;

            _log.Debug($"Lobby state changed to: {newState}");
            
            _dataCollector.Current.LobbyState = newState;
            _dataCollector.TriggerDataChanged();
        }
    }
}