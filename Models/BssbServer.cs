using System.Net;
using Hive.Versioning;
using Newtonsoft.Json;
using ServerBrowser.Models.JsonConverters;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models
{
    /// <summary>
    /// Basic server information.
    /// </summary>
    /// <see cref="BssbServerDetail">Extended model</see>
    public class BssbServer : JsonObject<BssbServer>
    {
        /// <summary>
        /// Server side identifier (hash key) for this game.
        /// </summary>
        [JsonProperty("Key")] public string? Key;
        [JsonProperty("ServerCode")] public string? ServerCode;
        [JsonProperty("OwnerId")] public string? OwnerId;
        [JsonProperty("HostSecret")] public string? HostSecret;
        [JsonProperty("ManagerId")] public string? ManagerId;
        [JsonProperty("PlayerLimit")] public int? PlayerLimit;
        [JsonProperty("GameplayMode")] public GameplayServerMode? GameplayMode;
        [JsonProperty("GameName")] public string? Name;
        [JsonProperty("LobbyState")] public MultiplayerLobbyState? LobbyState;
        [JsonProperty("Platform")] public string? ReportingPlatformId;

        [JsonProperty("MasterServerEp")] [JsonConverter(typeof(MasterServerEndPointConverter))]
        public MasterServerEndPoint? MasterServerEndPoint;

        [JsonProperty("Endpoint")] [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint? EndPoint;

        [JsonProperty("MpCoreVersion")] [JsonConverter(typeof(HiveVersionJsonConverter))]
        public Version? MultiplayerCoreVersion;

        [JsonProperty("MpExVersion")] [JsonConverter(typeof(HiveVersionJsonConverter))]
        public Version? MultiplayerExtensionsVersion;

        [JsonIgnore] public bool IsQuickPlay => GameplayMode == GameplayServerMode.Countdown;

        [JsonIgnore]
        public bool IsOfficial => !IsBeatTogetherHost &&
                                  (MasterServerEndPoint == null ||
                                   MasterServerEndPoint.hostName.EndsWith(".beatsaber.com"));

        [JsonIgnore] public bool IsBeatTogetherHost => OwnerId == "ziuMSceapEuNN7wRGQXrZg";

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