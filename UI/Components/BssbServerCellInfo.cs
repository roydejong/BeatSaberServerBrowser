using BeatSaberMarkupLanguage.Components;
using ServerBrowser.Models;

namespace ServerBrowser.UI.Components
{
    public class BssbServerCellInfo : CustomListTableData.CustomCellInfo
    {
        public BssbServer Server { get; set; }
        
        public BssbServerCellInfo(BssbServer server)
            : base(server.Name, server.BrowserDetailTextWithDifficulty, null)
        {
            Server = server;
        }
    }
}