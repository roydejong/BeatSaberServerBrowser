using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ServerBrowser.Models
{
    public class BssbServerDetail : BssbServer
    {
        [JsonProperty("Players")] public List<BssbServerPlayer> Players = new();
        [JsonProperty("PlayerCount")] public int PlayerCount => Players.Count(p => !p.IsGhost);
        [JsonIgnore] public BssbServerPlayer? LocalPlayer => Players.FirstOrDefault(p => p.IsMe);
        [JsonIgnore] public BssbServerPlayer? HostPlayer => Players.FirstOrDefault(p => p.IsHost);
        [JsonIgnore] public BssbServerPlayer? PartyLeaderPlayer => Players.FirstOrDefault(p => p.IsPartyLeader);
    }
}