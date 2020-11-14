using Newtonsoft.Json;
using ServerBrowser.Core;
using System.Collections.Generic;

namespace ServerBrowser.Core
{
    public class ServerBrowseResult
    {
        public int Count { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public List<HostedGameData> Lobbies { get; set; }
        public string Message { get; set; } = null;

        public static ServerBrowseResult FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ServerBrowseResult>(json);
        }
    }
}
