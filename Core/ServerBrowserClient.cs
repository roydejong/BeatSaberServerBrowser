using System;
using System.Threading;
using System.Threading.Tasks;
using IPA.Utilities;
using ServerBrowser.Utils;
using SiraUtil.Logging;
using Zenject;
using Version = Hive.Versioning.Version;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Integrates server browser ops with the game client and MultiplayerCore.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerBrowserClient : IInitializable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly IPlatformUserModel _platformUserModel = null!;
        [Inject] private readonly INetworkConfig _networkConfig = null!;
        [Inject] private readonly BssbDataCollector _dataCollector = null!;

        public void Initialize()
        {
            LoadModdedStatus();
            _ = LoadPlatformUserInfo();
        }

        #region Game Version

        public string GameVersionRaw => UnityGame.GameVersion.ToString();

        public string GameVersionNoSuffix
        {
            get
            {
                var raw = GameVersionRaw;
                var separatorIdx = raw.IndexOf('_');
                return separatorIdx > 0 ? raw.Substring(0, separatorIdx) : raw;
            }
        }

        #endregion
        
        #region Mod Status

        public Version? MultiplayerCoreVersion { get; private set; } = null;
        public Version? MultiplayerExtensionsVersion { get; private set; } = null;

        private void LoadModdedStatus()
        {
            MultiplayerCoreVersion = ModCheck.MultiplayerCore.InstalledVersion;
            MultiplayerExtensionsVersion = ModCheck.MultiplayerExtensions.InstalledVersion;

            _log.Debug($"Checked related mods (multiplayerCoreVersion={MultiplayerCoreVersion}, " +
                      $"multiplayerExtensionsVersion={MultiplayerExtensionsVersion?.ToString() ?? "Not installed"})");
        }

        #endregion

        #region Master Server

        public string MasterGraphUrl => _networkConfig.graphUrl;  

        public string MasterGraphHostname
        {
            get
            {
                try
                {
                    var uri = new Uri(MasterGraphUrl);
                    return uri.Host;
                }
                catch (UriFormatException)
                {
                    return MasterGraphUrl;
                }
            }
        }

        public bool UsingOfficialMaster => MasterGraphHostname.EndsWith(".oculus.com");

        public bool UsingBeatTogetherMaster => MasterGraphHostname.EndsWith(".beattogether.systems");

        public string MasterStatusUrl => _networkConfig.multiplayerStatusUrl;

        #endregion

        #region Server Name

        public string PreferredServerName => (!string.IsNullOrWhiteSpace(_config.ServerName)
            ? _config.ServerName
            : DefaultServerName)!;

        public string DefaultServerName => PlatformUserInfo is not null
            ? $"{PlatformUserInfo.userName}'s game"
            : "Untitled Beat Game";

        #endregion

        #region UserInfo

        public UserInfo? PlatformUserInfo { get; private set; } = null!;
        public UserInfo.Platform? Platform => PlatformUserInfo?.platform;
        public string PlatformKey => Platform?.ToString() ?? "unknown";

        private async Task LoadPlatformUserInfo(CancellationToken ctx = default)
        {
            try
            {
                PlatformUserInfo = await _platformUserModel.GetUserInfo(ctx);

                if (PlatformUserInfo == null)
                {
                    _log.Warn("Failed to load platform user info!");
                    return;
                }

                _log.Debug($"Loaded platform user info (platform={PlatformUserInfo.platform}, " +
                           $"userName={PlatformUserInfo.userName}, platformUserId={PlatformUserInfo.platformUserId})");

                _dataCollector.Current.ReportingPlatformKey = PlatformKey;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to load platform user info: {ex}");
            }
        }

        #endregion
    }
}