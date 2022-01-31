using BeatSaberMarkupLanguage.Components;
using ServerBrowser.Models;

namespace ServerBrowser.UI.Components
{
    public class BssbServerCellInfo : CustomListTableData.CustomCellInfo
    {
        public readonly BssbServer Server;
        
        public BssbServerCellInfo(BssbServer server) : base(server.Name, server.LobbyStateText, null)
        {
            Server = server;
        }
    }
}