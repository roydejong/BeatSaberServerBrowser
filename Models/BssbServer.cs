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
        [JsonProperty("OwnerId")] public string? OwnerId;
        /// <summary>
        /// Unique lobby secret. Can be used to access specific Quick Play lobbies via matchmaking.
        /// </summary>
        [JsonProperty("HostSecret")] public string? HostSecret;
        /// <summary>
        /// User ID for the party leader (player in charge of the lobby).
        /// </summary>
        [JsonProperty("ManagerId")] public string? ManagerId;
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
        /// HTTP URL for the cover art associated with the level, if any.
        /// Provided by the API when querying lobbies.
        /// </summary>
        [JsonProperty("CoverUrl")] public string? CoverArtUrl;

        /// <summary>
        /// The endpoint for the master server this lobby exists on.
        /// </summary>
        [JsonProperty("MasterServerEp")] [JsonConverter(typeof(MasterServerEndPointConverter))]
        public MasterServerEndPoint? MasterServerEndPoint;

        /// <summary>
        /// The endpoint for the dedicated server instance this lobby is hosted on.
        /// </summary>
        [JsonProperty("Endpoint")] [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint? EndPoint;

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

        [JsonIgnore] public bool IsQuickPlay => GameplayMode == GameplayServerMode.Countdown;

        [JsonIgnore]
        public bool IsOfficial => !IsBeatTogetherHost &&
                                  (MasterServerEndPoint == null ||
                                   MasterServerEndPoint.hostName.EndsWith(".beatsaber.com"));

        [JsonIgnore] public bool IsBeatTogetherHost => OwnerId == "ziuMSceapEuNN7wRGQXrZg";

        /// <summary>
        /// Identifies what type of server this is for announce messages.
        /// </summary>
        [JsonProperty("ServerType")]
        public string ServerTypeCode
        {
            get
            {
                if (IsOfficial)
                    return IsQuickPlay ? "vanilla_quickplay" : "vanilla_dedicated";
                else if (IsBeatTogetherHost)
                    return IsQuickPlay ? "beattogether_quickplay" : "beattogether_dedicated";
                else
                    return "unknown";
            }
        }
    }
}