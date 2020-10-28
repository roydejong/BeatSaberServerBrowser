using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyBrowserMod.Core
{
    public class LobbyAnnounceInfo
    {
        public string ServerCode;
        public string GameName;
        public string OwnerId;
        public string OwnerName;
        public int PlayerCount;
        public int PlayerLimit;
        public bool IsModded;

        public string Describe()
        {
            var moddedDescr = IsModded ? "Modded" : "Vanilla";
            return $"{GameName} ({PlayerCount}/{PlayerLimit} players, {moddedDescr})";
        }
    }
}
