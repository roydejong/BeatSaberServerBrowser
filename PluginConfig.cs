using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBrowser
{
    public class PluginConfig
    {
        public bool LobbyAnnounceToggle { get; set; } = true;
        public string CustomGameName { get; set; } = null;
        public bool JoinNotificationsEnabled { get; set; } = true;
    }
}
