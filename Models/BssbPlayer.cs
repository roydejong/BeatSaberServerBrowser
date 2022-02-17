using Newtonsoft.Json;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models
{
    /// <summary>
    /// Persistent player information that is the same between games.
    /// </summary>
    /// <see cref="BssbServerPlayer">Extended model</see>
    public class BssbPlayer : JsonObject<BssbPlayer>
    {
        [JsonProperty("UserId")] public string? UserId;
        [JsonProperty("UserName")] public string? UserName;
        [JsonProperty("PlatformType")] public string? PlatformType;
        [JsonProperty("PlatformUserId")] public string? PlatformUserId;
        [JsonProperty("AvatarData")] public MultiplayerAvatarData? AvatarData;
    }
}