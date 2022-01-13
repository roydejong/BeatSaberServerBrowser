namespace ServerBrowser.Models
{
    /// <summary>
    /// Extended player information, relative to an active server session.
    /// </summary>
    public class BssbServerPlayer : BssbPlayer
    {
        public int SortIndex;
        public bool IsHost;
        public bool IsPartyLeader;
        public bool IsAnnouncing;
        public float CurrentLatency;

        /// <summary>
        /// Extra text shown on the player list in the detail view.
        /// </summary>
        public string ListText
        {
            get
            {
                if (IsHost)
                    return "Server Host";
                if (IsPartyLeader)
                    return "Party Leader";
                if (IsAnnouncing)
                    return "Game Announcer";
                return $"Player {SortIndex + 1}";
            }
        }

        public static BssbServerPlayer FromConnectedPlayer(IConnectedPlayer player, bool isPartyLeader = false)
        {
            return new BssbServerPlayer()
            {
                SortIndex = player.sortIndex,
                IsHost = player.isConnectionOwner,
                IsPartyLeader = isPartyLeader,
                IsAnnouncing = false,
                CurrentLatency = player.currentLatency
            };
        }
    }
}