using System;
using System.Threading.Tasks;
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

        public async void Initialize()
        {
            LoadModdedStatus();

            await LoadPlatformUserInfo();
        }

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
        public bool IsSteam => Platform == UserInfo.Platform.Steam;
        public bool IsOculus => Platform == UserInfo.Platform.Oculus;

        private async Task LoadPlatformUserInfo()
        {
            PlatformUserInfo = await _platformUserModel.GetUserInfo();

            if (PlatformUserInfo == null)
            {
                _log.Warn("Failed to load platform user info!");
                return;
            }

            _log.Debug($"Loaded platform user info (platform={PlatformUserInfo.platform}, " +
                      $"userName={PlatformUserInfo.userName}, platformUserId={PlatformUserInfo.platformUserId})");

            _dataCollector.Current.ReportingPlatformKey = PlatformKey;
        }

        #endregion
    }
}