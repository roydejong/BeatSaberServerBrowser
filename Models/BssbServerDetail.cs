using System.Collections.Generic;

namespace ServerBrowser.Models
{
    public class BssbServerDetail : BssbServer
    {
        public List<BssbServerPlayer> Players;

        public BssbServerDetail()
        {
            Players = new();
        }
    }
}