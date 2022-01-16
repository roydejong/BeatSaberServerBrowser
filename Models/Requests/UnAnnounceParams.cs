using Newtonsoft.Json;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models.Requests
{
    public class UnAnnounceParams : JsonObject<UnAnnounceParams>
    {
        [JsonProperty("SelfUserId")] public string? SelfUserId;
        [JsonProperty("HostUserId")] public string? HostUserId;
        [JsonProperty("HostSecret")] public string? HostSecret;

        [JsonIgnore] public bool IsComplete => SelfUserId != null && HostUserId != null && HostSecret != null;
    }
}