using ServerBrowser.Core;

namespace ServerBrowser.Game
{
    public static class MpConnect
    {
        public const string OFFICIAL_MASTER_SUFFIX = ".mp.beatsaber.com";
        public const string OFFICIAL_MASTER_OCULUS = "oculus.production.mp.beatsaber.com";
        public const string OFFICIAL_MASTER_STEAM = "steam.production.mp.beatsaber.com";

        public const int DEFAULT_MASTER_PORT = 2328;

        public static void Join(HostedGameData game)
        {
            if (game.MasterServerHost == null || game.MasterServerHost.EndsWith(OFFICIAL_MASTER_SUFFIX))
            {
                // Game is hosted on the player platform's official master server
                if (_usingModdedServer || _officialEndPoint == null)
                {
                    // If we normally use a modded server, we need to fall back to official servers manually
                    if (Plugin.PlatformId == Plugin.PLATFORM_OCULUS)
                    {
                        SetMasterServerOverride(OFFICIAL_MASTER_OCULUS);
                    }
                    else
                    {
                        SetMasterServerOverride(OFFICIAL_MASTER_STEAM);
                    }
                }
                else
                {
                    // Clearing the override should fall back to the correct official server
                    ClearMasterServerOverride();
                }
            }
            else
            {
                // Game is hosted on a custom master server, we need to override
                SetMasterServerOverride(game.MasterServerHost, game.MasterServerPort.HasValue ? game.MasterServerPort.Value : DEFAULT_MASTER_PORT);
            }

            // Trigger the actual join via server code
            MpModeSelection.ConnectToHostedGame(game);
        }

        #region Master Server Management
        private static MasterServerEndPoint _officialEndPoint;
        private static MasterServerEndPoint _moddedEndPoint;
        private static bool _usingModdedServer;

        public static MasterServerEndPoint OverrideEndPoint { get; private set; } = null;
        public static MasterServerEndPoint LastUsedMasterServer { get; private set; } = null;

        public static bool ShouldDisableCertificateValidation
        {
            get
            {
                // We should disable certificate validation (X509CertificateUtilityPatch) if we are overriding to unofficial masters
                return OverrideEndPoint != null && !OverrideEndPoint.hostName.EndsWith(OFFICIAL_MASTER_SUFFIX);
            }
        }

        internal static void ReportCurrentMasterServerValue(MasterServerEndPoint currentEndPoint)
        {
            var isFirstReport = (LastUsedMasterServer == null);

            LastUsedMasterServer = currentEndPoint;

            if (OverrideEndPoint != null && currentEndPoint.Equals(OverrideEndPoint))
            {
                // This is our own override, not useful information
                return;
            }

            var hostName = currentEndPoint.hostName;

            if (hostName.EndsWith(OFFICIAL_MASTER_SUFFIX))
            {
                // This is the official / default master server (likely not using a server mod)
                _officialEndPoint = currentEndPoint;
                _usingModdedServer = false;

                if (isFirstReport)
                {
                    Plugin.Log?.Info($"Using an official master server: {_officialEndPoint}");
                }

                return;
            }

            // This is neither our override nor an official server, which means another mod is doing this
            _moddedEndPoint = currentEndPoint;
            _usingModdedServer = true;

            if (isFirstReport)
            {
                Plugin.Log?.Warn($"Using a modded master server: {_moddedEndPoint}");
            }
        }

        public static void SetMasterServerOverride(string hostName, int port = DEFAULT_MASTER_PORT)
        {
            SetMasterServerOverride(new MasterServerEndPoint(hostName, port));
        }

        public static void SetMasterServerOverride(MasterServerEndPoint overrideEndPoint)
        {
            if (OverrideEndPoint == null || !OverrideEndPoint.Equals(overrideEndPoint))
            {
                Plugin.Log?.Info($"Setting master server override: {overrideEndPoint}");
                OverrideEndPoint = overrideEndPoint;
            }
        }

        public static void ClearMasterServerOverride()
        {
            if (OverrideEndPoint != null)
            {
                Plugin.Log?.Info($"Clearing master server override");
                OverrideEndPoint = null;
            }
        }
    }
    #endregion
}
