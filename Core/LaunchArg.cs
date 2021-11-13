using System;

namespace ServerBrowser.Core
{
    public static class LaunchArg
    {
        private const string LaunchArgPrefix = "-bssb:";
        
        public static string Format(string bssbKey)
        {
            return $"{LaunchArgPrefix}{bssbKey}";
        }

        public static string? TryParseBssbKey(string launchArg)
        {
            if (!launchArg.StartsWith(LaunchArgPrefix))
                return null;

            return launchArg.Substring(LaunchArgPrefix.Length).Trim();
        }

        public static string? TryGetBssbKeyFromEnv()
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (!arg.StartsWith(LaunchArgPrefix))
                    continue;

                return TryParseBssbKey(arg);
            }

            return null;
        }
    }
}