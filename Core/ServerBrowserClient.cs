using System.Threading.Tasks;
using HarmonyLib;
using MultiplayerCore.Patchers;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Integrates server browser ops with the game client and MultiplayerCore.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerBrowserClient : IInitializable, IAffinity
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly IPlatformUserModel _platformUserModel = null!;
        [Inject] private readonly NetworkConfigPatcher _mpCoreNetConfig = null!;
        [Inject] private readonly BssbDataCollector _dataCollector = null!;

        public UserInfo? PlatformUserInfo;

        private MasterServerEndPoint? _lastUsedMasterServerEndPoint = null;

        public async void Initialize()
        {
            await LoadPlatformUserInfo();
        }

        private async Task LoadPlatformUserInfo()
        {
            PlatformUserInfo = await _platformUserModel.GetUserInfo();

            if (PlatformUserInfo == null)
            {
                _log.Warn("Failed to load platform user info!");
                return;
            }

            _log.Info($"Loaded platform user info (platform={PlatformUserInfo.platform}, " +
                      $"userName={PlatformUserInfo.userName}, platformUserId={PlatformUserInfo.platformUserId})");

            _dataCollector.Current.ReportingPlatformId = PlatformId;
        }

        public MasterServerEndPoint? MasterServerEndPoint =>
            _mpCoreNetConfig.MasterServerEndPoint ?? _lastUsedMasterServerEndPoint;

        public string? MasterServerHost => MasterServerEndPoint?.hostName;
        public bool UsingOfficialMaster => MasterServerHost == null || MasterServerHost.EndsWith(".beatsaber.com");
        public bool UsingBeatTogetherMaster => MasterServerHost?.EndsWith(".beattogether.systems") ?? false;

        public string PreferredServerName => (!string.IsNullOrWhiteSpace(_config.ServerName)
            ? _config.ServerName
            : DefaultServerName)!;

        public string DefaultServerName => PlatformUserInfo is not null
            ? $"{PlatformUserInfo.userName}'s game"
            : "Untitled Beat Game";

        public UserInfo.Platform? Platform => PlatformUserInfo?.platform;
        public string PlatformId => Platform?.ToString() ?? "unknown";
        public bool IsSteam => Platform == UserInfo.Platform.Steam;
        public bool IsOculus => Platform == UserInfo.Platform.Oculus;

        [AffinityPostfix]
        [AffinityPatch(typeof(NetworkConfigSO), nameof(NetworkConfigSO.masterServerEndPoint),
            AffinityMethodType.Getter)]
        [AffinityPriority(Priority.Last)] // we want to read the final used value that may be influenced by mods
        private void HandleMasterServerEndpointRead(MasterServerEndPoint __result)
        {
            if (_lastUsedMasterServerEndPoint?.Equals(__result) ?? false)
                return;

            _lastUsedMasterServerEndPoint = __result;
            
            _log.Info($"Game used master server: {__result} (IsOfficial={UsingOfficialMaster}, " +
                      $"IsBeatTogether={UsingBeatTogetherMaster})");
        }
    }
}