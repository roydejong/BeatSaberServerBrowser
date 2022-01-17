using System.Collections.Generic;
using System.Linq;

namespace ServerBrowser.Models
{
    public class BssbServerDetail : BssbServer
    {
        public List<BssbServerPlayer> Players = new();

        public BssbServerPlayer? LocalPlayer => Players.FirstOrDefault(p => p.IsMe);
        public BssbServerPlayer? HostPlayer => Players.FirstOrDefault(p => p.IsHost);
        public BssbServerPlayer? PartyLeaderPlayer => Players.FirstOrDefault(p => p.IsPartyLeader);
    }
}