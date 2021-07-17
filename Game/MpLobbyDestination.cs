using IPA.Utilities;

namespace ServerBrowser.Game
{
    public class MpLobbyDestination : SelectMultiplayerLobbyDestination
    {
        public MpLobbyDestination(string serverCode, string? hostSecret) : base(serverCode)
        {
            if (hostSecret != null)
                this.SetField<SelectMultiplayerLobbyDestination, string>("lobbySecret", hostSecret);
        }
    }
}