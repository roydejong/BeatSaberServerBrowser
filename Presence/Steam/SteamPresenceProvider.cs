using System;
using System.IO;
using ServerBrowser.Game.Models;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ServerBrowser.Presence.Steam
{
    public class SteamPresenceProvider : IPresenceProvider
    {
        private GameObject? _tickObject = null;
        private IDisposable? _gameRichPresenceJoinRequestedCallback = null;

        public bool GetIsAvailable()
        {
            return Plugin.Config.SteamRichPresenceEnabled && TryDetectSteamDll();
        }

        private bool TryDetectSteamDll()
        {
            var testPath = Path.GetFullPath(Path.Combine(CustomLevelPathHelper.baseProjectPath,
                "Plugins", "x86_64", "steam_api64.dll"));
            var testResult = File.Exists(testPath);
            
            Plugin.Log.Info($"[SteamPresenceProvider] TryDetectSteamDll (path={testPath}, result={testResult})");

            return testResult;
        }

        #region Start/Stop
        public void Start()
        {
            if (!SteamManager.Initialized && !SteamAPI.Init())
            {
                Plugin.Log?.Error("[SteamPresenceProvider] SteamAPI.Init() failed");
                return;
            }
            
            Plugin.Log?.Info("[SteamPresenceProvider] SteamAPI initialized");

            _tickObject = new GameObject("ServerBrowser_SteamPresenceProvider");
            _tickObject.AddComponent<SteamPresenceEventBehavior>();
            Object.DontDestroyOnLoad(_tickObject);
            
            _gameRichPresenceJoinRequestedCallback = Callback<GameRichPresenceJoinRequested_t>.Create(
                OnGameRichPresenceJoinRequested
            );
            
            Plugin.Log?.Info("[SteamPresenceProvider] SteamAPI initialized - ran through");
        }

        public void Stop()
        {
            if (_tickObject is not null)
            {
                Object.Destroy(_tickObject);
                _tickObject = null;
            }
            
            if (_gameRichPresenceJoinRequestedCallback is not null)
            {
                _gameRichPresenceJoinRequestedCallback.Dispose();
                _gameRichPresenceJoinRequestedCallback = null;
            }
        }
        #endregion

        #region Events from Steam
        private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t pCallback)
        {
            var secret = pCallback.m_rgchConnect;
            Plugin.Log?.Info($"[SteamPresenceProvider] OnGameRichPresenceJoinRequested (secret={secret})");
            Plugin.PresenceManager?.JoinFromSecret(secret);
        }
        #endregion

        #region Update
        public void Update(MultiplayerActivity? activity)
        {
            if (activity is null)
            {
                ClearActivity();
                return;
            }

            // TODO Proper status text
            // TODO Will HostUserId work as a key for BeatTogether etc? Also, uhh: should it just be text?
            
            SteamFriends.SetRichPresence("connect", activity.BssbGame?.Key ?? "");
            SteamFriends.SetRichPresence("steam_player_group", activity.HostUserId ?? "");
            SteamFriends.SetRichPresence("steam_player_group_size", activity.CurrentPlayerCount.ToString());
            
            SteamFriends.SetRichPresence("status", "Test status");
            
            Plugin.Log?.Info("[SteamPresenceProvider] SteamAPI did set activity");
        }

        private void ClearActivity()
        {
            SteamFriends.SetRichPresence("connect", "");
            SteamFriends.SetRichPresence("steam_player_group", "");
            SteamFriends.SetRichPresence("steam_player_group_size", "");
            
            Plugin.Log?.Info("[SteamPresenceProvider] SteamAPI did clear activity");
        }
        #endregion
    }
}