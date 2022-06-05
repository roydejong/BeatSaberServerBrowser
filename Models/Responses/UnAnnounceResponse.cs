using Newtonsoft.Json;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models.Responses
{
    public class UnAnnounceResponse : JsonObject<UnAnnounceResponse>
    {
        public string? Result;
        public bool CanRetry;

        [JsonIgnore] public bool IsOk => Result == "ok";
    }
}