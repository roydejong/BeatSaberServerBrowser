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
        [Inject] private readonly MasterServerRepository _masterServerRepository = null!;

        public void ConfigureOfficialGamelift()
        {
            _log.Info("Apply network config: Official GameLift");
            _mpCoreNetworkConfig.UseOfficialServer();
        }

        public async Task<ConfigResult> ConfigureCustomMasterServer(string graphUrl, string? statusUrl,
            CancellationToken cancellationToken)
        {
            var masterServerInfo = _masterServerRepository.GetMasterServerInfo(graphUrl);
            
            if (statusUrl == null && masterServerInfo is { StatusUrl: not null })
                // Use status URL from known master server
                statusUrl = masterServerInfo.StatusUrl;
            
            if (statusUrl == null)
            {
                // No status URL known at all, we can only use basic config
                _log.Info($"Apply network config: Modded Master Server (no status URL), {graphUrl}");
                _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, null);
                return new ConfigResult();
            }
            
            var serverStatus = _mpCoreStatusRepository.GetStatusForUrl(statusUrl);

            // Prefer to get fresh master server status
            _log.Info($"Checking master server status: {statusUrl}");
            
            _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, statusUrl); // set temporary basic config for status check
            
            var status = await _multiplayerStatusModel.GetMultiplayerStatusAsync(cancellationToken);
            var statusIsFresh = false;

            if (status is MpStatusData mpStatusData)
            {
                serverStatus = mpStatusData;
                statusIsFresh = true;
            }
            else
            {
                _log.Warn($"Failed to get updated master server status for: {statusUrl}");
            }

            // Apply network config with the best available data
            _log.Info($"Apply network config: Modded Master Server, {graphUrl}");
            
            var playerLimit = serverStatus?.maxPlayers ?? masterServerInfo?.MaxPlayers ?? 5;
            var useSsl = serverStatus?.useSsl ?? masterServerInfo?.UseSsl ?? false;
            
            _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, statusUrl, playerLimit, null, useSsl);
            
            // Provide fresh status info to our list of master servers
            if (serverStatus != null && statusIsFresh)
                _masterServerRepository.ProvideStatusInfo(graphUrl, statusUrl, serverStatus);

            // Check server status (base game or MultiplayerCore may raise an error based on the status response)
            var isUnavailable = MultiplayerUnavailableReasonMethods.TryGetMultiplayerUnavailableReason(serverStatus,
                out var multiplayerUnavailableReason);
            
            // Combined result
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