using System;
using System.Collections.Generic;
using System.IO;
using ServerBrowser.Core;
using ServerBrowser.Game.Models;
using Steamworks;
using UnityEngine;
using static MultiplayerLobbyConnectionController;
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
            
            Plugin.Log.Debug($"[SteamPresenceProvider] TryDetectSteamDll (path={testPath}, result={testResult})");

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
            
            Plugin.Log?.Debug("[SteamPresenceProvider] SteamAPI initialized");

            _tickObject = new GameObject("ServerBrowser_SteamPresenceProvider");
            _tickObject.AddComponent<SteamPresenceEventBehavior>();
            Object.DontDestroyOnLoad(_tickObject);
            
            _gameRichPresenceJoinRequestedCallback = Callback<GameRichPresenceJoinRequested_t>.Create(
                OnGameRichPresenceJoinRequested
            );
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
            var connectArg = pCallback.m_rgchConnect;
            var parsedKey = LaunchArg.TryParseBssbKey(connectArg);
            
            Plugin.Log?.Info($"[SteamPresenceProvider] OnGameRichPresenceJoinRequested" +
                             $" (secret={connectArg}, parsed={parsedKey})");
            
            if (parsedKey is not null)
                Plugin.PresenceManager?.JoinFromSecret(parsedKey);
        }
        #endregion

        #region Update
        public void Update(MultiplayerActivity? activity)
        {
            if (activity?.BssbGame is null || activity.ConnectionType == LobbyConnectionType.None)
            {
                ClearActivity();
                return;
            }
            
            // These rich presence keys will group players together in the friend list & allows joins/invites
            SteamFriends.SetRichPresence("connect",  LaunchArg.Format(activity.BssbGame.Key));
            SteamFriends.SetRichPresence("steam_player_group", activity.BssbGame.Key);
            SteamFriends.SetRichPresence("steam_player_group_size", activity.CurrentPlayerCount.ToString());
            
            /***
             * Note: we have limited control over the rich presence status text.
             * Beat Saber's steam localization does not allow variables, so we can't display anything special.
             * 
             * Our "status" text will show up in the "view game info" dialog in the Steam friends list though.
             */

            var statusParts = new List<string>();
            statusParts.Add("Server Browser:");

            if (activity.IsInGameplay)
            {
                statusParts.Add("Playing level");

                if (activity.CurrentLevel is not null)
                {
                    statusParts.Add("-");
                    statusParts.Add($"{activity.CurrentLevel.songAuthorName} - {activity.CurrentLevel.songName}");
                    statusParts.Add($"[{activity.CurrentDifficultyName}]");
                }
            }
            else
            {
                statusParts.Add("In lobby");
            }

            statusParts.Add($"({activity.Name}, {activity.CurrentPlayerCount}/{activity.MaxPlayerCount} players)");

            var statusText = String.Join(" ", statusParts);
            SteamFriends.SetRichPresence("status", statusText);
            Plugin.Log?.Debug($"[SteamPresenceProvider] SteamAPI did set activity (status={statusText})");
        }

        private void ClearActivity()
        {
            SteamFriends.SetRichPresence("connect", null);
            SteamFriends.SetRichPresence("steam_player_group", null);
            SteamFriends.SetRichPresence("steam_player_group_size", null);
            SteamFriends.SetRichPresence("status", null);
            
            Plugin.Log?.Debug("[SteamPresenceProvider] SteamAPI did clear activity");
        }
        #endregion
    }
}