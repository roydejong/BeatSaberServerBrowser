using System.Collections.Generic;

namespace ServerBrowser.Models
{
    public class BssbServerDetail : BssbServer
    {
        public List<BssbServerPlayer> Players = new();
        public BssbServerLevel? Level;
    }
}