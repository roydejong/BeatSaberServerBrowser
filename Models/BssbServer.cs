using System.Net;
using Newtonsoft.Json;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models
{
    /// <summary>
    /// Basic server information.
    /// </summary>
    /// <see cref="BssbServerDetail">Extended model</see>
    public class BssbServer : JsonObject<BssbServer>
    {
        [JsonProperty("key")] public string? Key;
        [JsonProperty("ownerId")] public string? OwnerId;
        [JsonProperty("serverCode")] public string? ServerCode;
        [JsonProperty("hostSecret")] public string? HostSecret;
        [JsonProperty("playerLimit")] public int? PlayerLimit;
        [JsonProperty("managerId")] public string? ManagerId;
        [JsonProperty("endpoint")] public IPEndPoint? EndPoint;
        [JsonProperty("masterServerEp")] public MasterServerEndPoint? MasterServerEndPoint;
        [JsonProperty("gameplayMode")] public GameplayServerMode? GameplayMode;
        [JsonProperty("gameName")] public string? Name;

        [JsonIgnore] public bool IsQuickPlay => GameplayMode == GameplayServerMode.Countdown;

        [JsonIgnore] public bool IsOfficial =>
            MasterServerEndPoint == null || MasterServerEndPoint.hostName.EndsWith(".beatsaber.com");

        [JsonIgnore] public bool IsBeatTogetherHost => OwnerId == "ziuMSceapEuNN7wRGQXrZg";

        [JsonProperty("serverType")]
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