using System;
using Discord;
using ServerBrowser.Utils;
using DiscordCore;
using JetBrains.Annotations;
using ServerBrowser.Assets;
using ServerBrowser.Game.Models;

namespace ServerBrowser.Presence.DiscordCore
{
    public class PresenceProvider : IPresenceProvider
    {
        private DiscordInstance _instance;

        public bool GetIsAvailable()
        {
            return ModChecker.DiscordCore.InstalledAndEnabled &&
                   ModChecker.DiscordCore.InstalledVersion >= new SemVer.Version("1.0.5");
        }

        #region Start/Stop
        public void Start()
        {
            _instance = DiscordManager.instance.CreateInstance(new DiscordSettings()
            {
                handleInvites = true,
                modIcon = Sprites.Portal,
                modId = Plugin.HarmonyId,
                modName = "Beat Saber Server Browser"
            });

            _instance.OnActivityInvite += OnDiscordActivityInvite;
            _instance.OnActivityJoin += OnDiscordActivityJoin;
            _instance.OnActivitySpectate += OnDiscordActivitySpectate;
            _instance.OnActivityJoinRequest += OnDiscordActivityJoinRequest;
        }

        public void Stop()
        {
            _instance.OnActivityInvite -= OnDiscordActivityInvite;
            _instance.OnActivityJoin -= OnDiscordActivityJoin;
            _instance.OnActivitySpectate -= OnDiscordActivitySpectate;
            _instance.OnActivityJoinRequest -= OnDiscordActivityJoinRequest;
            
            _instance.ClearActivity();
            _instance.DestroyInstance();
            _instance = null;
        }
        #endregion

        #region Events from Discord
        private void OnDiscordActivityInvite(ActivityActionType type, ref User user, ref Activity activity)
        {
            Plugin.Log?.Warn("OnDiscordActivityInvite");
        }
        
        private void OnDiscordActivityJoinRequest(ref User user)
        {
            Plugin.Log?.Warn("OnDiscordActivityJoinRequest");
        }

        private void OnDiscordActivitySpectate(string secret)
        {
            Plugin.Log?.Warn("OnDiscordActivitySpectate");
        }

        private void OnDiscordActivityJoin(string secret)
        {
            Plugin.Log?.Warn("OnDiscordActivityJoin");
        }
        #endregion

        #region Update
        public void Update(MultiplayerActivity? activity)
        {
            if (activity == null || !activity.IsInMultiplayer)
            {
                _instance.ClearActivity();
                return;
            }

            // Base
            var discordActivity = new Activity()
            {
                Party = new ActivityParty()
                {
                    Id = activity.HostUserId,
                    Size = new PartySize()
                    {
                        CurrentSize = activity.CurrentPlayerCount,
                        MaxSize = activity.MaxPlayerCount
                    }
                },
                Secrets = new ActivitySecrets()
                {
                    Match = activity.HostSecret,
                    Join = activity.ServerCode,
                    Spectate = activity.ServerCode
                },
                Timestamps = new ActivityTimestamps()
                {
                    Start = DateTime.Now.Ticks,
                }
            };

            // Title
            if (activity.IsQuickPlay)
            {
                discordActivity.State = $"In Quick Play ({activity.DifficultyMaskName})";
            }
            else if (activity.IsHost)
            {
                discordActivity.State = "Hosting Multiplayer Game";
            }
            else
            {
                discordActivity.State = "In Multiplayer Game";
            }

            // Subtitle
            if (activity.IsInGameplay && activity.CurrentLevel != null)
            {
                discordActivity.Details =
                    $"Playing {activity.CurrentLevel.songName} ({activity.CurrentDifficultyName})";
            }
            else
            {
                discordActivity.Details = $"In lobby ({activity.CurrentPlayerCount}/{activity.MaxPlayerCount} players)";
            }

            // Apply
            Plugin.Log.Info($"[PresenceProvider] Setting Discord activity " +
                            $"(State={discordActivity.State}, Details={discordActivity.Details})");
            _instance.UpdateActivity(discordActivity);
        }
        #endregion
    }
}