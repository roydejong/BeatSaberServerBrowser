using BeatSaberMarkupLanguage.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBrowser.Core
{
    public class LobbyAnnounceInfo
    {
        public int? Id { get; set; }
        public string ServerCode { get; set; }
        public string GameName { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int PlayerCount { get; set; }
        public int PlayerLimit { get; set; }
        public bool IsModded { get; set; }

        public string Describe()
        {
            var moddedDescr = IsModded ? "Modded" : "Vanilla";
            return $"{GameName} ({PlayerCount}/{PlayerLimit} players, {moddedDescr})";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
