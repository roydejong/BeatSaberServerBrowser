using UnityEngine;

namespace ServerBrowser.Core
{
    public class UpdateTimer : MonoBehaviour
    {
        private const float TickIntervalSecs = 120f;

        #region Public API
        private static GameObject? _gameObject;
        
        private static void CreateTimerObject()
        {
            if (_gameObject is not null)
                return;

            _gameObject = new GameObject("ServerBrowser_UpdateTimer");
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
            if (_gameObject is null)
                CreateTimerObject();
            
            _gameObject!.SetActive(true);
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

        public void OnDisable()
        {
            CancelInvoke(nameof(Tick));
        }

        private void Tick()
        {
            if ((GameStateManager.Activity.IsHost && Plugin.Config.LobbyAnnounceToggle) ||
                (GameStateManager.Activity.IsQuickPlay && Plugin.Config.ShareQuickPlayGames))
            {
                GameStateManager.HandleUpdate();
            }
        }
        #endregion
    }
}
