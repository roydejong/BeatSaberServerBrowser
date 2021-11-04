using Steamworks;
using UnityEngine;

namespace ServerBrowser.Presence.Steam
{
    public class SteamPresenceEventBehavior : MonoBehaviour
    {
        public const float TickTime = 0.5f;
        
        public void OnEnable()
        {
            InvokeRepeating(nameof(EventTick), 3.0f, TickTime);
        }

        public void OnDisable()
        {
            CancelInvoke(nameof(EventTick));
        }

        private void EventTick()
        {
            if (!SteamManager.Initialized)
                return;
            
            SteamAPI.RunCallbacks();
        }
    }
}