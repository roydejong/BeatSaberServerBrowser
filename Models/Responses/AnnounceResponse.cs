using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models.Responses
{
    public class AnnounceResponse : JsonObject<AnnounceResponse>
    {
        public bool Success;
        public string? Key;
    }
}