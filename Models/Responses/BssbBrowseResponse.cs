using System.Collections.Generic;
using JetBrains.Annotations;

namespace ServerBrowser.Models.Responses
{
    [UsedImplicitly]
    public class BssbBrowseResponse
    {
        public List<BssbLobby> Lobbies { get; set; }
    }
}