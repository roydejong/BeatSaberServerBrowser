using System.Threading;
using System.Threading.Tasks;
using BGLib.Polyglot;
using JetBrains.Annotations;
using MultiplayerCore.Models;
using MultiplayerCore.Patchers;
using MultiplayerCore.Repositories;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Data
{
    [UsedImplicitly]
    public class MultiplayerConfigManager
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly IMultiplayerStatusModel _multiplayerStatusModel = null!;
        [Inject] private readonly NetworkConfigPatcher _mpCoreNetworkConfig = null!;
        [Inject] private readonly MpStatusRepository _mpCoreStatusRepository = null!;

        public void ConfigureOfficialGamelift()
        {
            _log.Info("Apply network config: Official GameLift");
            _mpCoreNetworkConfig.UseOfficialServer();
        }

        public async Task<ConfigResult> ConfigureCustomMasterServer(string graphUrl, string? statusUrl,
            CancellationToken cancellationToken)
        {
            if (statusUrl == null)
            {
                // No status URL provided, we can only use basic config
                _log.Info($"Apply network config: Modded Master Server (no status URL), {graphUrl}");
                _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, null);
                return new ConfigResult();
            }
            
            var serverStatus = _mpCoreStatusRepository.GetStatusForUrl(statusUrl);

            // Prefer to get fresh master server status
            _log.Info($"Checking master server status: {statusUrl}");
            _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, statusUrl);
            var status = await _multiplayerStatusModel.GetMultiplayerStatusAsync(cancellationToken);
            
            if (status is MpStatusData mpStatusData)
                serverStatus = mpStatusData;
            else
                _log.Warn($"Failed to get updated master server status for: {statusUrl}");

            _log.Info($"Apply network config: Modded Master Server, {graphUrl}");
            _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, statusUrl, serverStatus?.maxPlayers, null,
                serverStatus?.useSsl ?? false);

            var isUnavailable = MultiplayerUnavailableReasonMethods.TryGetMultiplayerUnavailableReason(serverStatus,
                out var multiplayerUnavailableReason);
            
            return new ConfigResult()
            {
                MpStatusData = serverStatus,
                UnavailableReason = isUnavailable ? multiplayerUnavailableReason : null,
                LocalizedMessage = serverStatus?.GetLocalizedMessage(Localization.Instance.SelectedLanguage)
            };
        }

        public class ConfigResult
        {
            public MpStatusData? MpStatusData { get; init; }
            public MultiplayerUnavailableReason? UnavailableReason { get; init; }
            public string? LocalizedMessage { get; init; }
        }
    }
}