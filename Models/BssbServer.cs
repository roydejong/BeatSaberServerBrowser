using System;
using Newtonsoft.Json;
using ServerBrowser.Models.Enums;
using ServerBrowser.Models.JsonConverters;
using ServerBrowser.Models.Utils;
using ServerBrowser.Utils;
using Version = Hive.Versioning.Version;

namespace ServerBrowser.Models
{
    /// <summary>
    /// Basic server information. Represents a single lobby instance.
    /// </summary>
    /// <see cref="BssbServerDetail">Extended model</see>
    public class BssbServer : JsonObject<BssbServer>
    {
        /// <summary>
        /// Server side identifier (hash key) for this lobby instance.
        /// </summary>
        [JsonProperty("Key")] public string Key = null!;

        /// <summary>
        /// Unique 5 character code assigned to the lobby by the master server.
        /// </summary>
        [JsonProperty("ServerCode")] public string? ServerCode;

        /// <summary>
        /// User ID for the lobby host (unique per dedicated server).
        /// </summary>
        [JsonProperty("OwnerId")] public string? RemoteUserId;

        /// <summary>
        /// User name for the lobby host (unique per dedicated server).
        /// </summary>
        [JsonProperty("OwnerName")] public string? RemoteUserName;

        /// <summary>
        /// Unique lobby secret. Can be used to access specific Quick Play lobbies via matchmaking.
        /// </summary>
        [JsonProperty("HostSecret")] public string? HostSecret;

        /// <summary>
        /// User ID for the party leader (player in charge of the lobby).
        /// </summary>
        [JsonProperty("ManagerId")] public string? ManagerId;

        /// <summary>
        /// The current amount of non-ghost players in this lobby.
        /// </summary>
        /// <remarks>
        /// This field is not filled by the mod/client.
        /// The BSSB API derives it from the "players" list in the detailed announce.
        /// </remarks>
        [JsonProperty("PlayerCount")] public int? ReadOnlyPlayerCount;
        
        /// <summary>
        /// The maximum amount of players permitted in this lobby.
        /// </summary>
        [JsonProperty("PlayerLimit")] public int? PlayerLimit;

        /// <summary>
        /// Server configuration mode, indicates what kind of lobby this is.
        ///  • Countdown: Quick Play lobby with vote-based levels and automatic countdown
        ///  • Managed: Custom game where one player, the party leader, is in control.
        ///  • QuickStartOneSong: Currently unused. Most likely a Quick Play mode for specific levels.
        /// </summary>
        [JsonProperty("GameplayMode")] public GameplayServerMode? GameplayMode;

        /// <summary>
        /// Server name as set by the lobby creator.
        /// </summary>
        [JsonProperty("GameName")] public string? Name;

        /// <summary>
        /// Current state of the lobby.
        /// </summary>
        [JsonProperty("LobbyState")] public MultiplayerLobbyState? LobbyState;

        /// <summary>
        /// The announcing player's platform key (e.g. "steam", "oculus").
        /// </summary>
        [JsonProperty("Platform")] public string? ReportingPlatformKey;

        /// <summary>
        /// Information for the current or most recently completed level.
        /// </summary>
        [JsonProperty("Level")] public BssbServerLevel? Level;

        /// <summary>
        /// Lobby difficulty (for Quick Play), or last played level difficulty.
        /// </summary>
        [JsonProperty("Difficulty")] public BssbDifficulty? LobbyDifficulty;
        
        /// <summary>
        /// Current or last played level difficulty.
        /// </summary>
        [JsonProperty("LevelDifficulty")] public BssbDifficulty? LevelDifficulty;

        /// <summary>
        /// Identifies what type of server this is for announce messages.
        /// </summary>
        [JsonProperty("ServerType")] public string? ServerTypeCode;

        /// <summary>
        /// The Graph API URL for the master server (1.29+).
        /// </summary>
        [JsonProperty("MasterGraphUrl")] public string? MasterGraphUrl;

        /// <summary>
        /// The multiplayer status check URL associated with the master server.
        /// </summary>
        [JsonProperty("MasterStatusUrl")] public string? MasterStatusUrl;

        /// <summary>
        /// The endpoint for the dedicated server instance this lobby is hosted on.
        /// </summary>
        [JsonProperty("Endpoint")] [JsonConverter(typeof(DnsEndPointConverter))]
        public DnsEndPoint? EndPoint;

        /// <summary>
        /// The announcer's game version, or a server's compatible game version.
        /// </summary>
        /// <remarks>
        /// This field is not filled by the mod/client.
        /// The BSSB API derives it from the user agent.
        /// </remarks>
        [JsonProperty("GameVersion")] [JsonConverter(typeof(HiveVersionConverter))]
        public Version? GameVersion;

        /// <summary>
        /// The announcer's installed or compatibility version of MultiplayerCore.
        /// MultiplayerCore is required (for custom songs, and for this mod).
        /// </summary>
        [JsonProperty("MpCoreVersion")] [JsonConverter(typeof(HiveVersionConverter))]
        public Version? MultiplayerCoreVersion;

        /// <summary>
        /// The announcer's installed or compatibility version of MultiplayerExtensions.
        /// MultiplayerExtensions is optional.
        /// </summary>
        [JsonProperty("MpExVersion")] [JsonConverter(typeof(HiveVersionConverter))]
        public Version? MultiplayerExtensionsVersion;
        
        /// <summary>
        /// Server type description, as generated by the API.
        /// </summary>
        /// <remarks>
        /// This field is not filled by the mod/client.
        /// The BSSB API derives it from announce data.
        /// </remarks>
        [JsonProperty("ServerTypeText")]
        public string? ServerTypeText;
        
        /// <summary>
        /// Master server description, as generated by the API.
        /// </summary>
        /// <remarks>
        /// This field is not filled by the mod/client.
        /// The BSSB API derives it from announce data.
        /// </remarks>
        [JsonProperty("MasterServerText")]
        public string? MasterServerText;
        
        [JsonProperty("FirstSeen")]
        public DateTime? ReadOnlyFirstSeen;
        
        [JsonProperty("LastUpdate")]
        public DateTime? ReadOnlyLastSeen;

        [JsonProperty("EncryptionMode")]
        public string? EncryptionMode;
        
        #region Local

        [JsonIgnore] public bool IsQuickPlay => GameplayMode == GameplayServerMode.Countdown;

        [JsonIgnore] public bool IsOfficial => IsAwsGameLiftHost;

        [JsonIgnore] public bool IsBeatTogetherHost => RemoteUserId == "ziuMSceapEuNN7wRGQXrZg";
        [JsonIgnore] public bool IsBeatUpServerHost => RemoteUserId?.StartsWith("beatupserver:", 
            StringComparison.OrdinalIgnoreCase) ?? false;
        [JsonIgnore] public bool IsBeatNetHost => RemoteUserId?.StartsWith("beatnet:", 
            StringComparison.OrdinalIgnoreCase) ?? false;
        [JsonIgnore] public bool IsAwsGameLiftHost => RemoteUserId?.StartsWith("arn:aws:gamelift:") ?? false;

        [JsonIgnore] public bool IsDirectConnect => (!IsOfficial && MasterGraphUrl is null && EndPoint is not null)
                                                    || IsBeatNetHost;

        [JsonIgnore]
        public string LobbyStateText
        {
            get
            {
                return LobbyState switch
                {
                    MultiplayerLobbyState.None => "None",
                    MultiplayerLobbyState.LobbySetup => "In lobby (setup)",
                    MultiplayerLobbyState.LobbyCountdown => "In lobby (countdown)",
                    MultiplayerLobbyState.GameStarting => "Level starting",
                    MultiplayerLobbyState.GameRunning => "Playing level",
                    MultiplayerLobbyState.Error => "Error",
                    _ => "Unknown"
                };
            }
        }

        [JsonIgnore]
        public string DualDifficultyFormatted
        {
            get
            {
                if (LobbyDifficulty is null)
                    return "New lobby";

                if (LobbyDifficulty.HasValue && LevelDifficulty.HasValue && LevelDifficulty != LobbyDifficulty)
                    // Have both lobby and level difficulty, but they diverge (happens with "All" difficulty lobbies)
                    return $"{LobbyDifficulty.Value.ToFormattedText()} ({LevelDifficulty.Value.ToFormattedText()})";
                
                // Have only basic lobby difficulty
                return $"{LobbyDifficulty.Value.ToFormattedText()}";
            }
        }

        [JsonIgnore]
        public string BrowserDetailTextWithDifficulty
        {
            get
            {
                if (LobbyDifficulty is null)
                    return BrowserDetailText;

                if (IsInLobby || LevelDifficulty is null)
                    return $"{DualDifficultyFormatted} : {BrowserDetailText}";
                else
                    return $"{DualDifficultyFormatted} : {BrowserDetailText}";
            }
        }

        [JsonIgnore]
        public string BrowserDetailText => (IsInGameplay && Level != null)
            ? $"Playing {Level.ListDescription}"
            : LobbyStateText;

        [JsonIgnore]
        public bool IsInLobby => LobbyState is MultiplayerLobbyState.None or MultiplayerLobbyState.Error
            or MultiplayerLobbyState.LobbyCountdown or MultiplayerLobbyState.LobbySetup;

        [JsonIgnore]
        public bool IsInGameplay =>
            LobbyState is MultiplayerLobbyState.GameStarting or MultiplayerLobbyState.GameRunning;

        [JsonIgnore]
        public TimeSpan? LobbyLifetime => ReadOnlyFirstSeen != null ? DateTime.Now - ReadOnlyFirstSeen : null;

        [JsonIgnore]
        public string LobbyLifetimeText => LobbyLifetime?.ToReadableString() ?? "Unknown";

        [JsonIgnore]
        public GameplayServerMode LogicalGameplayServerMode
        {
            get
            {
                if (GameplayMode is not null)
                    return GameplayMode.Value;
                
                return IsQuickPlay ? GameplayServerMode.Countdown : GameplayServerMode.Managed;
            }
        }
        
        [JsonIgnore]
        public SongSelectionMode LogicalSongSelectionMode
        {
            get
            {
                if (GameplayMode == GameplayServerMode.Countdown || IsQuickPlay)
                    return SongSelectionMode.Vote;
                
                return SongSelectionMode.OwnerPicks;
            }
        }

        /// <summary>
        /// Gets whether ENet SSL (DTLS) should be used for this server.
        /// </summary>
        [JsonIgnore]
        public bool UseENetSSL => EncryptionMode == "enet_dtls";

        /// <summary>
        /// Flags whether this server was discovered via local discovery.
        /// </summary>
        [JsonIgnore]
        public bool IsLocallyDiscovered { get; set; }

        #endregion

        #region Sort

        /// <summary>
        /// Scores the lobby for preferential sorting; a guess on how likely it is the player wants to join.
        /// </summary>
        public int PreferentialSortScore
        {
            get
            {
                var score = 0;
                
                // Boost: locally discovered servers, very likely the player wants to see these
                if (IsLocallyDiscovered)
                    score += 1000;
                
                // Drop: full servers
                if (ReadOnlyPlayerCount >= PlayerLimit)
                    score -= 250;
                // Boost: servers with only one player, they need us!
                else if (ReadOnlyPlayerCount == 1)
                    score += 125;
                
                // Boost: "all" difficulty lobbies
                if (LobbyDifficulty == BssbDifficulty.All)
                    score += 75;
                
                // Drop: games in progress
                if (IsInGameplay)
                    score -= 100;
                
                // Drop: official servers
                if (IsOfficial)
                    score -= 50;
                
                // Small differentiator: prefer "normal" sized lobbies, where we prefer higher slot counts
                if (PlayerLimit is > 0 and < 5)
                    score += 25 + (PlayerLimit.Value * 10);

                return score;
            }
        }

        #endregion
    }
}