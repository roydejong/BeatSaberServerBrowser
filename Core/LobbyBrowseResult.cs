using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyBrowserMod.Core
{
    public class LobbyBrowseResult
    {
        public int Count { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public List<LobbyAnnounceInfo> Lobbies { get; set; }

        public static LobbyBrowseResult FromJson(string json)
        {
            return JsonConvert.DeserializeObject<LobbyBrowseResult>(json);
        }
    }
}
