using System.Text;
using ServerBrowser.Utils;

namespace ServerBrowser.UI.Utils
{
    public static class ConnectionErrorTextProvider
    {
        public static string Generate(MultiplayerLobbyConnectionController.LobbyConnectionType connectionType,
            ConnectionFailedReason reason)
        {
            var reasonInt = (int) reason;
            var reasonStr = reason.ToStringWithSpaces();

            var msg = new StringBuilder();
            msg.AppendLine($"Error CFR-{reasonInt} ({reasonStr})");
            
            switch (reason)
            {
                default:
                case ConnectionFailedReason.None: // CFR-0
                case ConnectionFailedReason.Unknown: // CFR-1
                    msg.AppendLine("An unknown error occurred. Please try again.");
                    break;
                
                case ConnectionFailedReason.ConnectionCanceled: // CFR-2
                    // user should not normally see this
                    msg.AppendLine("You cancelled the connection."); 
                    break;
                
                case ConnectionFailedReason.ServerUnreachable: // CFR-3
                    msg.AppendLine("Could not connect to the game server.");
                    break;
                
                case ConnectionFailedReason.ServerAlreadyExists: // CFR-4
                    // user should not normally see this
                    msg.AppendLine("The server already exists (this should never happen).");
                    break;
                    
                case ConnectionFailedReason.ServerDoesNotExist: // CFR-5
                    msg.AppendLine("The server code is invalid, or the lobby already ended.");
                    break;
                    
                case ConnectionFailedReason.ServerAtCapacity: // CFR-6
                    msg.AppendLine("The server is already full.");
                    break;
                    
                case ConnectionFailedReason.VersionMismatch: // CFR-7
                    // user should not normally see this
                    msg.AppendLine("Your game version is not compatible with this server.");
                    break;
                    
                case ConnectionFailedReason.InvalidPassword: // CFR-8
                    // user should not normally see this
                    msg.AppendLine("Please enter the correct password to connect to this server.");
                    break;
                
                case ConnectionFailedReason.MultiplayerApiUnreachable: // CFR-9
                    // this is now used for both GameLift API failures and general master server connect fails
                    msg.AppendLine("Could not connect to master server.");
                    break;
                
                case ConnectionFailedReason.AuthenticationFailed: // CFR-10
                    msg.AppendLine("Log in failed. You may need to restart your game and/or Steam.");
                    break;
                
                case ConnectionFailedReason.NetworkNotConnected: // CFR-11
                    msg.AppendLine("Network error. Please check your connection and try again.");
                    break;
                    
                case ConnectionFailedReason.CertificateValidationFailed: // CFR-12
                    msg.AppendLine("The master server's certificate could not be validated.");
                    break;
                    
                case ConnectionFailedReason.ServerIsTerminating: // CFR-13
                    msg.AppendLine("This game server is already shutting down.");
                    break;
                
                case ConnectionFailedReason.Timeout: // CFR-14
                    msg.AppendLine("The connection to the server timed out.");
                    break;
                
                case ConnectionFailedReason.FailedToFindMatch: // CFR-15
                    msg.AppendLine("Matchmaking failed. Please try again.");
                    break;
            }

            return msg.ToString();
        }
    }
}