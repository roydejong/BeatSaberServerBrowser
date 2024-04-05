using System.Threading;
using System.Threading.Tasks;
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

        public async Task<MpStatusData?> ConfigureCustomMasterServer(string graphUrl, string? statusUrl,
            CancellationToken cancellationToken)
        {
            if (statusUrl == null)
            {
                // No status URL provided, we can only use basic config
                _log.Info($"Apply network config: Modded Master Server (no status URL), {graphUrl}");
                _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, null);
                return null;
            }
            
            var serverStatus = _mpCoreStatusRepository.GetStatusForUrl(statusUrl);
            if (serverStatus == null)
            {
                _log.Info($"Checking master server status: {statusUrl}");
                _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, statusUrl);
                var status = await _multiplayerStatusModel.GetMultiplayerStatusAsync(cancellationToken);
                if (status is MpStatusData mpStatusData)
                    serverStatus = mpStatusData;
            }
            if (serverStatus == null)
                _log.Warn($"Failed to get master server status for {statusUrl}");
            
            _log.Info($"Apply network config: Modded Master Server, {graphUrl}");
            _mpCoreNetworkConfig.UseCustomApiServer(graphUrl, statusUrl, serverStatus?.maxPlayers, null,
                serverStatus?.useSsl ?? false);
            return serverStatus;
        }
    }
}