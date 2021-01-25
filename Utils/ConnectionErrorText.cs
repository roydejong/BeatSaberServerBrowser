using System.Text;
using System.Text.RegularExpressions;

namespace ServerBrowser.Utils
{
    public static class ConnectionErrorText
    {
        public static string Generate(ConnectionFailedReason reason)
        {
            var reasonInt = (int)reason;
            var reasonStr = Regex.Replace(reason.ToString(), "(\\B[A-Z])", " $1"); // insert spaces in error token for readability

            var msg = new StringBuilder();
            msg.AppendLine($"Error CFR-{reasonInt} ({reasonStr})");

            switch (reason)
            {
                case ConnectionFailedReason.ServerUnreachable:
                    msg.AppendLine("Could not connect to the host.");
                    break;
                case ConnectionFailedReason.ServerDoesNotExist:
                    msg.AppendLine("It looks like this game has already ended.");
                    break;
                case ConnectionFailedReason.ServerAtCapacity:
                    msg.AppendLine("This game is full.");
                    break;
                case ConnectionFailedReason.NetworkNotConnected:
                case ConnectionFailedReason.MasterServerUnreachable:
                    msg.AppendLine("Could not connect to master server, check your connection and try again.");
                    break;
                case ConnectionFailedReason.VersionMismatch:
                    msg.AppendLine("Make sure you and the host are using the same game version.");
                    break;
                case ConnectionFailedReason.MasterServerNotAuthenticated:
                    msg.AppendLine("Could not authenticate master server, you may need to restart the game.");
                    break;
            }

            return msg.ToString();
        }
    }
}
