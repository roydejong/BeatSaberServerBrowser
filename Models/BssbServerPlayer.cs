using System;
using BeatSaber.AvatarCore;
using BeatSaber.BeatAvatarAdapter;
using BeatSaber.BeatAvatarSDK;
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

        public static BssbServerPlayer FromConnectedPlayer(IConnectedPlayer player)
        {
            // Multiple avatar systems are supported; it looks like Meta avatars will be a thing in the future...
            // For the website we're only really interested in "BeatAvatars".

            AvatarData? beatAvatarData = null;

            try
            {
                foreach (var avatarData in player.multiplayerAvatarsData.multiplayerAvatarsData)
                    if (avatarData.avatarTypeIdentifierHash == BeatAvatarSystemId.hash)
                        beatAvatarData = avatarData.data != null ? avatarData.CreateAvatarData() : null;
            }
            catch (NullReferenceException)
            {
                // Unclear why this happens, can't null check value structs, whatever
            }

            return new BssbServerPlayer()
            {
                UserId = player.userId,
                UserName = player.userName,
                AvatarData = beatAvatarData,
                SortIndex = player.sortIndex,
                IsMe = player.isMe,
                IsHost = player.isConnectionOwner,
                IsPartyLeader = false,
                IsAnnouncing = false,
                CurrentLatency = player.currentLatency
            };
        }

        private static readonly AvatarSystemIdentifier BeatAvatarSystemId = new("BeatAvatarSystem");
    }
}