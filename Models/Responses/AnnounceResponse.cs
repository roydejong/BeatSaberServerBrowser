using Newtonsoft.Json;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models.Responses
{
    public class AnnounceResponse : JsonObject<AnnounceResponse>
    {
        [JsonProperty("Success")] public bool Success;
        [JsonProperty("Key")] public string? Key;
        [JsonProperty("Message")] public string? ServerMessage;
    }
}