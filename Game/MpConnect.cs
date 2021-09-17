using System.Text;
using ServerBrowser.Core;
using ServerBrowser.Utils;

namespace ServerBrowser.Game
{
    public static class MpConnect
    {
        public const string OFFICIAL_MASTER_SUFFIX = ".mp.beatsaber.com";

        public const string OFFICIAL_MASTER_OCULUS = "oculus.production.mp.beatsaber.com";
        public const string OFFICIAL_MASTER_STEAM = "steam.production.mp.beatsaber.com";
        public const string OFFICIAL_MASTER_PS4 = "ps4.production.mp.beatsaber.com";
        public const string OFFICIAL_MASTER_TEST = "staging.mp.beatsaber.com";

        public const int DEFAULT_MASTER_PORT = 2328;

        public static void Join(HostedGameData game)
        {
            // MpEx version check
            if (game.MpExVersion != null)
            {
                var ourMpExVersion = ModCheck.MultiplayerExtensions.InstalledVersion;

                if (ourMpExVersion == null || !ourMpExVersion.Equals(game.MpExVersion))
                {
                    var ourMpExVersionStr = (ourMpExVersion != null ? ourMpExVersion.ToString() : "Not installed");
                    var theirMpExVersionStr = game.MpExVersion.ToString();

                    Plugin.Log.Warn($"Blocking game join because of MultiplayerExtensions version mismatch " +
                        $"(ours: {ourMpExVersionStr}, theirs: {theirMpExVersionStr})");

                    var mpExError = new StringBuilder();
                    mpExError.AppendLine($"MultiplayerExtensions version difference detected!");
                    mpExError.AppendLine($"Please ensure you and the host are both using the latest version.");
                    mpExError.AppendLine();
                    mpExError.AppendLine($"Your version: {ourMpExVersionStr}");
                    mpExError.AppendLine($"Their version: {theirMpExVersionStr}");

                    MpModeSelection.PresentConnectionFailedError
                    (
                        errorTitle: "Incompatible game",
                        errorMessage: mpExError.ToString(),
                        canRetry: false
                    );
                    return;
                }
            }

            // Master server switching
            if (game.MasterServerHost == null || game.MasterServerHost.EndsWith(OFFICIAL_MASTER_SUFFIX))
            {
                // Game is hosted on the player platform's official master server
                if (_usingModdedServer || _officialEndPoint == null)
                {
                    // If we normally use a modded server (e.g. because BeatTogether is installed), we need to now force-connect to our official server
                    switch (MpLocalPlayer.Platform)
                    {
                        case UserInfo.Platform.Oculus:
                            SetMasterServerOverride(OFFICIAL_MASTER_OCULUS);
                            break;
                        case UserInfo.Platform.PS4:
                            // lmao
                            SetMasterServerOverride(OFFICIAL_MASTER_PS4);
                            break;
                        case UserInfo.Platform.Test:
                            // hmmm
                            SetMasterServerOverride(OFFICIAL_MASTER_TEST);
                            break;
                        default:
                        case UserInfo.Platform.Steam:
                        case null:
                            SetMasterServerOverride(OFFICIAL_MASTER_STEAM);
                            break;
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
            _ = MpModeSelection.ConnectToHostedGame(game);
        }

        #region Master Server Management
        private static MasterServerEndPoint _officialEndPoint;
        private static MasterServerEndPoint _moddedEndPoint;
        private static bool _usingModdedServer;

        public static MasterServerEndPoint? OverrideEndPoint { get; private set; } = null;
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
                    Plugin.Log?.Info($"Default master server appears to be official: {_officialEndPoint}");
                }

                return;
            }

            // This is neither our override nor an official server, which means another mod is doing this
            _moddedEndPoint = currentEndPoint;
            _usingModdedServer = true;

            if (isFirstReport)
            {
                Plugin.Log?.Warn($"Default master server appears to be modded: {_moddedEndPoint}");
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
                Plugin.Log?.Info($"Setting master server override now: {overrideEndPoint}");
                OverrideEndPoint = overrideEndPoint;
            }
        }

        public static void ClearMasterServerOverride()
        {
            if (OverrideEndPoint != null)
            {
                Plugin.Log?.Info($"Stopped overriding master server");
                OverrideEndPoint = null;
            }
        }
    }
    #endregion
}
