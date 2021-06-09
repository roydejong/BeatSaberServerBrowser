using ServerBrowser.Harmony;
using System;
using System.Threading;
using UnityEngine;

namespace ServerBrowser.Core
{
    public class UpdateTimer : MonoBehaviour
    {
        private const float TickIntervalSecs = 120f;

        #region Public API
        private static GameObject _gameObject;
        
        public static void CreateTimerObject()
        {
            if (_gameObject != null)
                return;

            _gameObject = new GameObject("ServerBrowserUpdateTimer");
            _gameObject.AddComponent<UpdateTimer>();
            
            DontDestroyOnLoad(_gameObject); // keep object across all scenes
        }

        public static void DestroyTimerObject()
        {
            if (_gameObject == null)
                return;
            
            Destroy(_gameObject);
            _gameObject = null;
        }
        
        public static void StartTimer()
        {
            if (_gameObject == null)
                CreateTimerObject();
            
            _gameObject.SetActive(true);
        }

        public static void StopTimer()
        {
            if (_gameObject == null)
                return;
            
            _gameObject.SetActive(false);
        }
        #endregion

        #region UpdateTimer MonoBehavior
        public void OnEnable()
        {
            InvokeRepeating(nameof(Tick), TickIntervalSecs, TickIntervalSecs);
        }

        private void Tick()
        {
            if (!Plugin.Config.LobbyAnnounceToggle)
                return;
            
            if (MpLobbyConnectionTypePatch.IsPartyHost || MpLobbyConnectionTypePatch.IsQuickplay)
            {
                // We are the host and announcing a game, or actively sharing a quickplay lobby
                // Send periodic keep-alive updates to the server
                Plugin.Log?.Debug("Host timer tick: sending periodic update to master server");
                GameStateManager.HandleUpdate();
            }
        }
        #endregion
    }
}
