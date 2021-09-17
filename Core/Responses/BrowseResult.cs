using System.Collections.Generic;
using ServerBrowser.Utils.Serialization;

namespace ServerBrowser.Core.Responses
{
    public class BrowseResult : JsonObject<BrowseResult>
    {
        public int Count { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public List<HostedGameData> Lobbies { get; set; }
        public string Message { get; set; } = null;
    }
}
