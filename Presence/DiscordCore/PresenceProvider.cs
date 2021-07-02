using Discord;
using DiscordCore;
using ServerBrowser.Game.Models;
using ServerBrowser.Utils;

namespace ServerBrowser.Presence.DiscordCore
{
    public class PresenceProvider : IPresenceProvider
    {
        private const long DiscordAppId = 860315531944001556;
        
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
                modId = Plugin.HarmonyId,
                modName = "Beat Saber Server Browser",
                appId = DiscordAppId
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
            Plugin.Log?.Debug($"[PresenceProvider] OnDiscordActivityInvite");
        }
        
        private void OnDiscordActivityJoinRequest(ref User user)
        {
            Plugin.Log?.Debug("[PresenceProvider] OnDiscordActivityJoinRequest");
        }

        private void OnDiscordActivitySpectate(string secret)
        {
            Plugin.Log?.Debug("[PresenceProvider] OnDiscordActivitySpectate");
            PresenceSecret.FromString(secret).Connect();
        }

        private void OnDiscordActivityJoin(string secret)
        {
            Plugin.Log?.Debug("[PresenceProvider] OnDiscordActivityJoin");
            PresenceSecret.FromString(secret).Connect();
        }
        #endregion

        #region Update
        public void Update(MultiplayerActivity? activity)
        {
            if (activity is not {IsInMultiplayer: true} || activity.ServerCode == null
                                                        || activity.SessionStartedAt == null)
            {
                // Not in a multiplayer activity, or not enough data to make secrets
                _instance.ClearActivity();
                return;
            }

            var matchSecrets = activity.GetPresenceSecrets();

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
                    Match = matchSecrets[(byte)PresenceSecret.PresenceSecretType.Match].ToString(),
                    Join = matchSecrets[(byte)PresenceSecret.PresenceSecretType.Join].ToString(),
                    Spectate = matchSecrets[(byte)PresenceSecret.PresenceSecretType.Spectate].ToString()
                },
                Timestamps = new ActivityTimestamps()
                {
                    Start = activity.SessionStartedAt.Value.ToUnixTime()
                },
                Assets = new ActivityAssets()
                {
                    LargeImage = "bslogo",
                    SmallImage = "inlobby"
                }
            };

            // Title
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

            // Subtitle
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

            // Apply
            Plugin.Log.Info($"[PresenceProvider] Setting Discord activity " +
                            $"(State={discordActivity.State}, Details={discordActivity.Details})");
            _instance.UpdateActivity(discordActivity);
        }
        #endregion
    }
}