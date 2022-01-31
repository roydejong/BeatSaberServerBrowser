using System.Net;
using Hive.Versioning;
using Newtonsoft.Json;
using ServerBrowser.Models.JsonConverters;
using ServerBrowser.Models.Utils;

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
        [JsonProperty("Key")] public string? Key;

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
        /// Lobby difficulty (for Quick Play), or last played difficulty.
        /// </summary>
        [JsonProperty("Difficulty")] public BeatmapDifficulty? Difficulty;

        /// <summary>
        /// Current or last played level id.
        /// Provided by the API when querying lobbies.
        /// </summary>
        [JsonProperty("LevelId")] public string? ReadOnlyLevelId;
        
        /// <summary>
        /// HTTP URL for the cover art associated with the level, if any.
        /// Provided by the API when querying lobbies.
        /// </summary>
        [JsonProperty("CoverUrl")] public string? ReadOnlyCoverArtUrl;

        /// <summary>
        /// Identifies what type of server this is for announce messages.
        /// </summary>
        [JsonProperty("ServerType")] public string? ServerTypeCode;

        /// <summary>
        /// The endpoint for the master server this lobby exists on.
        /// </summary>
        [JsonProperty("MasterServerEp")] [JsonConverter(typeof(DnsEndPointConverter))]
        public DnsEndPoint? MasterServerEndPoint;

        /// <summary>
        /// The endpoint for the dedicated server instance this lobby is hosted on.
        /// </summary>
        [JsonProperty("Endpoint")] [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint? EndPoint;

        /// <summary>
        /// The announcer's game version, or a server's compatible game version.
        /// </summary>
        /// <remarks>
        /// This field is not filled by the mod/client.
        /// The BSSB API derives it from the user agent.
        /// </remarks>
        [JsonProperty("GameVersion")] [JsonConverter(typeof(HiveVersionJsonConverter))]
        public Version? GameVersion;

        /// <summary>
        /// The announcer's installed or compatibility version of MultiplayerCore.
        /// MultiplayerCore is required (for custom songs, and for this mod).
        /// </summary>
        [JsonProperty("MpCoreVersion")] [JsonConverter(typeof(HiveVersionJsonConverter))]
        public Version? MultiplayerCoreVersion;

        /// <summary>
        /// The announcer's installed or compatibility version of MultiplayerExtensions.
        /// MultiplayerExtensions is optional.
        /// </summary>
        [JsonProperty("MpExVersion")] [JsonConverter(typeof(HiveVersionJsonConverter))]
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

        [JsonIgnore] public bool IsQuickPlay => GameplayMode == GameplayServerMode.Countdown;

        [JsonIgnore]
        public bool IsOfficial => !IsBeatTogetherHost &&
                                  (IsGameLiftHost || MasterServerEndPoint == null ||
                                   MasterServerEndPoint.hostName.EndsWith(".beatsaber.com"));

        [JsonIgnore] public bool IsBeatTogetherHost => RemoteUserId == "ziuMSceapEuNN7wRGQXrZg";
        [JsonIgnore] public bool IsGameLiftHost => RemoteUserId?.StartsWith("arn:aws:gamelift:") ?? false;

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
        public string DifficultyName
        {
            get
            {
                return Difficulty switch
                {
                    BeatmapDifficulty.Easy => "Easy",
                    BeatmapDifficulty.Normal => "Normal",
                    BeatmapDifficulty.Hard => "Hard",
                    BeatmapDifficulty.Expert => "Expert",
                    BeatmapDifficulty.ExpertPlus => "Expert+",
                    _ => "Unknown"
                };
            }
        }

        [JsonIgnore]
        public string DifficultyNameWithColor
        {
            get
            {
                return Difficulty switch
                {
                    BeatmapDifficulty.Easy => "<color=#3cb371>Easy</color>",
                    BeatmapDifficulty.Normal => "<color=#59b0f4>Normal</color>",
                    BeatmapDifficulty.Hard => "<color=#ff6347>Hard</color>",
                    BeatmapDifficulty.Expert => "<color=#bf2a42>Expert</color>",
                    BeatmapDifficulty.ExpertPlus => "<color=#8f48db>Expert+</color>",
                    _ => "<color=#bcbdc2>Unknown</color>"
                };
            }
        }
    }
}