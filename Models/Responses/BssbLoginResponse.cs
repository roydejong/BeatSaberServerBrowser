using JetBrains.Annotations;

namespace ServerBrowser.Models.Responses
{
    [UsedImplicitly]
    public class BssbLoginResponse
    {
        /// <summary>
        /// Whether the login request was valid and points to a valid player profile.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Whether the user has successfully authenticated themselves as the player via token or Steam/Oculus ticket.
        /// </summary>
        public bool Authenticated { get; set; }
        /// <summary>
        /// Server error for logging purposes, if any.
        /// </summary>
        public string? ErrorMessage { get; set; }
        /// <summary>
        /// The platform avatar URL for the player.
        /// </summary>
        public string? AvatarUrl { get; set; }
    }
}