using Newtonsoft.Json;

namespace ServerBrowser.Models
{
    /// <summary>
    /// Extended player information, relative to an active server session.
    /// </summary>
    public class BssbServerPlayer : BssbPlayer
    {
        [JsonProperty("SortIndex")] public int SortIndex;
        [JsonProperty("IsMe")] public bool IsMe;
        [JsonProperty("IsHost")] public bool IsHost;
        [JsonProperty("IsPartyLeader")] public bool IsPartyLeader;
        [JsonProperty("IsAnnouncer")] public bool IsAnnouncing;
        [JsonProperty("Latency")] public float CurrentLatency;

        /// <summary>
        /// Indicates whether this player is invisible in the lobby.
        /// Invisible/ghost players do not take up a slot count.
        /// </summary>
        [JsonIgnore]
        public bool IsGhost => SortIndex < 0;
        
        /// <summary>
        /// Extra text shown on the player list in the detail view.
        /// </summary>
        [JsonIgnore]
        public string ListText
        {
            get
            {
                if (IsHost)
                    return "Server Host";
                if (IsPartyLeader)
                    return "Party Leader";
                if (IsAnnouncing)
                    return "Announcer";
                return $"Player";
            }
        }

        public static BssbServerPlayer FromConnectedPlayer(IConnectedPlayer player, bool isPartyLeader = false)
        {
            return new BssbServerPlayer()
            {
                UserId = player.userId,
                UserName = player.userName,
                AvatarData = player.multiplayerAvatarData,
                SortIndex = player.sortIndex,
                IsMe = player.isMe,
                IsHost = player.isConnectionOwner,
                IsPartyLeader = false,
                IsAnnouncing = false,
                CurrentLatency = player.currentLatency
            };
        }
    }
}