using IPA.Utilities;

namespace ServerBrowser.Game
{
    public class MpLobbyDestination 
    {
        public static SelectMultiplayerLobbyDestination Create(string serverCode, string? hostSecret)
        {
            var destination = new SelectMultiplayerLobbyDestination(serverCode);
            
            if (hostSecret != null)
                destination.SetField("lobbySecret", hostSecret);
            
            return destination;
        }
    }
}