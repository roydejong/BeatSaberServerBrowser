using Discord;
using DiscordCore;
using ServerBrowser.Game.Models;
using ServerBrowser.Utils;
using static ServerBrowser.Presence.PresenceSecret;

namespace ServerBrowser.Presence.DiscordCore
{
    public class PresenceProvider : IPresenceProvider
    {
        private const long DiscordAppId = 860315531944001556;
        
        private object? _discordInstance = null;

        public bool GetIsAvailable()
        {
            return Plugin.Config.DiscordRichPresenceEnabled
                   && ModChecker.DiscordCore.InstalledAndEnabled
                   && ModChecker.DiscordCore.InstalledVersion >= new SemVer.Version("1.0.5");
        }

        #region Start/Stop
        public void Start()
        {
            var discord = DiscordManager.instance.CreateInstance(new DiscordSettings()
            {
                handleInvites = true,
                modId = Plugin.HarmonyId,
                modName = "Beat Saber Server Browser",
                appId = DiscordAppId
            });

            discord.OnActivityInvite += OnDiscordActivityInvite;
            discord.OnActivityJoin += OnDiscordActivityJoin;
            discord.OnActivitySpectate += OnDiscordActivitySpectate;
            discord.OnActivityJoinRequest += OnDiscordActivityJoinRequest;

            _discordInstance = discord;
        }

        public void Stop()
        {
            var discord = _discordInstance as DiscordInstance;
            
            discord.OnActivityInvite -= OnDiscordActivityInvite;
            discord.OnActivityJoin -= OnDiscordActivityJoin;
            discord.OnActivitySpectate -= OnDiscordActivitySpectate;
            discord.OnActivityJoinRequest -= OnDiscordActivityJoinRequest;
            
            discord.ClearActivity();
            discord.DestroyInstance();
            
            _discordInstance = null;
        }
        #endregion

        #region Events from Discord
        private void OnDiscordActivityInvite(ActivityActionType type, ref User user, ref Activity activity)
        {
            Plugin.Log?.Info($"[PresenceProvider] OnDiscordActivityInvite (user: {user.Username})");
        }
        
        private void OnDiscordActivityJoinRequest(ref User user)
        {
            Plugin.Log?.Info($"[PresenceProvider] OnDiscordActivityJoinRequest (user: {user.Username})");
        }

        private void OnDiscordActivitySpectate(string secret)
        {
            Plugin.Log?.Info($"[PresenceProvider] OnDiscordActivitySpectate (secret: {secret})");
            PresenceSecret.FromString(secret).Connect();
        }

        private void OnDiscordActivityJoin(string secret)
        {
            Plugin.Log?.Info($"[PresenceProvider] OnDiscordActivityJoin (secret: {secret})");
            PresenceSecret.FromString(secret).Connect();
        }
        #endregion

        #region Update
        public void Update(MultiplayerActivity? activity)
        {
            var discord = _discordInstance as DiscordInstance;

            if (activity is null || !activity.InOnlineMenu)
            {
                discord.ClearActivity();
                return;
            }

            // Create base activity
            var discordActivity = new Activity()
            {
                State = "In Online menu",
                Details = null,
                Assets = new ActivityAssets()
                {
                    LargeImage = "bslogo",
                    SmallImage = null
                },
                Type = ActivityType.Playing,
                ApplicationId = DiscordAppId,
                Name = activity.Name
            };
            
            // Extend with session start time
            if (activity.SessionStartedAt != null)
            {
                discordActivity.Timestamps = new ActivityTimestamps()
                {
                    Start = activity.SessionStartedAt.Value.ToUnixTime()
                };
            }
            
            // Extend with lobby details, if we have one
            if (activity.IsInMultiplayer && activity.ServerCode != null)
            {
                // Add party details
                discordActivity.Party = new ActivityParty()
                {
                    Id = activity.HostUserId,
                    Size = new PartySize()
                    {
                        CurrentSize = activity.CurrentPlayerCount,
                        MaxSize = activity.MaxPlayerCount
                    }
                };
                
                // Add activity secret
                if (activity.CurrentPlayerCount >= activity.MaxPlayerCount)
                {
                    // Lobby full, add spectate secret
                    discordActivity.Secrets = new ActivitySecrets()
                    {
                        Spectate = activity.GetPresenceSecret(PresenceSecretType.Spectate).ToString()
                    };
                }
                else
                {
                    // Lobby has space, add join secret
                    discordActivity.Secrets = new ActivitySecrets()
                    {
                        Join = activity.GetPresenceSecret(PresenceSecretType.Join).ToString()
                    };
                }

                // State text & primary asset
                if (activity.IsQuickPlay)
                {
                    discordActivity.State = $"In Quick Play ({activity.DifficultyMaskName})";
                    discordActivity.Assets.LargeText = "Quick Play";
                }
                else if (activity.IsHost)
                {
                    discordActivity.State = $"Hosting {activity.Name}";
                    discordActivity.Assets.LargeText = "Custom Game (Host)";
                }
                else
                {
                    discordActivity.State = $"In {activity.Name}";
                    discordActivity.Assets.LargeText = "Custom Game (Player)";
                }
                
                // Detail text & secondary asset
                if (activity.IsInGameplay && activity.CurrentLevel != null)
                {
                    discordActivity.Details =
                        $"Playing \"{activity.CurrentLevel.songName}\" ({activity.CurrentDifficultyName})";
                    discordActivity.Assets.SmallImage = "lightsaber";
                    discordActivity.Assets.SmallText = "Playing level";
                }
                else
                {
                    discordActivity.Details = $"In lobby";
                    discordActivity.Assets.SmallImage = "inlobby";
                    discordActivity.Assets.SmallText = "In lobby";
                }
            }
            
            // Apply
            Plugin.Log.Info($"[PresenceProvider] Setting Discord activity " +
                            $"(State={discordActivity.State}, Details={discordActivity.Details})");
            
            discord.UpdateActivity(discordActivity);
        }
        #endregion
    }
}