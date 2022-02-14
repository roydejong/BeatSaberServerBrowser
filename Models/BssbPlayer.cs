using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models
{
    /// <summary>
    /// Persistent player information that is the same between games.
    /// </summary>
    /// <see cref="BssbServerPlayer">Extended model</see>
    public class BssbPlayer : JsonObject<BssbPlayer>
    {
        public string? UserId;
        public string? UserName;
        public MultiplayerAvatarData? AvatarData;
    }
}