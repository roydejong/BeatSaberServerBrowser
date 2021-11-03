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
            
            Plugin.Log.Info($"[SteamPresenceProvider] SteamPresenceEventBehavior -> OnEnable()");
        }

        private void EventTick()
        {
            if (SteamManager.Initialized)
            {
                Plugin.Log.Info($"[SteamPresenceProvider] SteamPresenceEventBehavior -> EventTick()");
                SteamAPI.RunCallbacks();
                
                SteamFriends.SetRichPresence("status", "Test status 1");
                SteamFriends.SetRichPresence("gamestatus", "Test status 2");
                SteamFriends.SetRichPresence("steam_display", null);
                SteamFriends.SetRichPresence("score", "123456");
            }
        }
    }
}