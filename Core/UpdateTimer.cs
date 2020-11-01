using ServerBrowser.Harmony;
using System;
using System.Threading;

namespace ServerBrowser.Core
{
    public static class UpdateTimer
    {
        private const int TICK_INTERVAL_SECS = 120;

        private static Timer _timer;

        public static void Start()
        {
            var timerIntervalMs = 1000 * TICK_INTERVAL_SECS;
            _timer = new Timer(Tick, null, timerIntervalMs, timerIntervalMs);
        }

        public static void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private static void Tick(object state)
        {
            if (MpLobbyConnectionTypePatch.IsPartyHost && Plugin.Config.LobbyAnnounceToggle)
            {
                // We are the host and announcing a game, send periodic keep-alive updates to the server
                Plugin.Log?.Debug("Host timer tick: sending periodic update to master server");
                GameStateManager.HandleUpdate();
            }
        }
    }
}
