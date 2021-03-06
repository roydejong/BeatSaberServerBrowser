﻿using System.Text;

namespace ServerBrowser.Utils
{
    public static class ConnectionErrorText
    {
        public static string Generate(ConnectionFailedReason reason)
        {
            var reasonInt = (int)reason;
            var reasonStr = reason.ToStringWithSpaces();

            var msg = new StringBuilder();
            msg.AppendLine($"Error CFR-{reasonInt} ({reasonStr})");

            switch (reason)
            {
                case ConnectionFailedReason.ServerUnreachable:
                    msg.AppendLine("Could not connect to the game host.");
                    break;
                case ConnectionFailedReason.ServerIsTerminating:
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
                case ConnectionFailedReason.MasterServerCertificateValidationFailed:
                    msg.AppendLine("Could not authenticate master server, you may need to restart the game.");
                    break;
            }

            return msg.ToString();
        }
    }
}
