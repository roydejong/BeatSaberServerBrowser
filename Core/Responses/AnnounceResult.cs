using ServerBrowser.Utils.Serialization;

namespace ServerBrowser.Core.Responses
{
    public class AnnounceResult : JsonObject<AnnounceResult>
    {
        public bool Success;
        public string Key;
    }
}