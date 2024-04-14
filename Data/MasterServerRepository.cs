using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MultiplayerCore.Models;
using Newtonsoft.Json;
using ServerBrowser.Models;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Data
{
    [UsedImplicitly]
    public class MasterServerRepository : IInitializable, IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbConfig _bssbConfig = null!;
        [Inject] private readonly BssbApi _bssbApi = null!;

        private Dictionary<string, MasterServerInfo> _masterServers = new();
        private bool _remoteUpdateInProgress;
        private bool _remoteUpdateSuccess;

        public void Initialize()
        {
            _bssbConfig.ReloadedEvent += HandleConfigReloaded;
            
            HandleConfigReloaded();
        }

        public void Dispose()
        {
            _bssbConfig.ReloadedEvent -= HandleConfigReloaded;
        }

        public MasterServerInfo? GetMasterServerInfo(string graphUrl) =>
            _masterServers.TryGetValue(graphUrl, out var masterServer) ? masterServer : null;

        private void HandleConfigReloaded()
        {
            // Assume config is the newest version; if we had updates we would have written them back to the config
            foreach (var cfgMaster in _bssbConfig.MasterServers)
                _masterServers[cfgMaster.GraphUrl] = cfgMaster;
        }
        
        /// <summary>
        /// Attempts to update the master server list from the BSSB API.
        /// This is a no-op if remote updates are disabled, already in progress, or completed in this session.
        /// </summary>
        public async Task TryRemoteUpdate()
        {
            if (!_bssbConfig.RemoteUpdateMasterServerList || _remoteUpdateInProgress || _remoteUpdateSuccess)
                // Remote updates are not enabled / already in progress / already completed successfully
                return;
            
            _remoteUpdateInProgress = true;
            try
            {
                var configResponse = await _bssbApi.SendConfigRequest();
                if (configResponse == null)
                {
                    _log.Warn("Failed to update master server list from BSSB API");
                    _remoteUpdateSuccess = false;
                    return;
                }

                foreach (var remoteServer in configResponse.MasterServers)
                {
                    var exists = _masterServers.TryGetValue(remoteServer.GraphUrl, out var existingServer);
                    if (exists && existingServer!.LastUpdated >= remoteServer.LastUpdated)
                        // Client knows this server and has fresher data
                        continue;
                    
                    _masterServers[remoteServer.GraphUrl] = remoteServer;
                }
                
                _remoteUpdateSuccess = true;
                
                WriteConfig();
            }
            finally
            {
                _remoteUpdateInProgress = false;
            }
        }

        public void ProvideStatusInfo(string graphUrl, string statusUrl, MpStatusData statusData)
        {
            var exists = _masterServers.TryGetValue(graphUrl, out var masterServerInfo);

            if (!exists)
            {
                masterServerInfo = new MasterServerInfo()
                {
                    GraphUrl = graphUrl
                };
            }

            masterServerInfo!.StatusUrl = statusUrl;
            masterServerInfo.UseSsl = statusData.useSsl;
            masterServerInfo.Name = statusData.name;
            masterServerInfo.Description = statusData.description;
            masterServerInfo.ImageUrl = statusData.imageUrl;
            masterServerInfo.MaxPlayers = statusData.maxPlayers;
            masterServerInfo.LastUpdated = DateTime.Now;
            
            _masterServers[graphUrl] = masterServerInfo;
            
            WriteConfig();
        }

        private void WriteConfig() =>
            _bssbConfig.MasterServers = _masterServers.Values.ToList();

        public class MasterServerInfo
        {
            /// <summary>
            /// Graph API URL for the master server.
            /// </summary>
            [JsonProperty("graphUrl")]
            public string GraphUrl { get; init; }

            /// <summary>
            /// Status API URL for the master server.
            /// </summary>
            [JsonProperty("statusUrl")]
            public string? StatusUrl { get; set; }
            
            /// <summary>
            /// Indicates whether dedicated servers hosted on this master server use SSL / DTLS encrypted connections.
            /// </summary>
            [JsonProperty("useSsl")]
            public bool UseSsl { get; set; }
            
            /// <summary>
            /// Display name for the master server (from extended status info).
            /// </summary>
            [JsonProperty("name")]
            public string? Name { get; set; }
            
            /// <summary>
            /// Description for the master server (from extended status info).
            /// </summary>
            [JsonProperty("description")]
            public string? Description { get; set; }
            
            /// <summary>
            /// Image URL for the master server (from extended status info).
            /// </summary>
            [JsonProperty("imageUrl")]
            public string? ImageUrl { get; set; }
            
            /// <summary>
            /// Maximum number of players for lobbies created on this master server.
            /// </summary>
            [JsonProperty("maxPlayers")]
            public int? MaxPlayers { get; set; }
            
            /// <summary>
            /// Indicates when this data was last updated (from a status request, either by the client or the BSSB API).
            /// </summary>
            public DateTime LastUpdated { get; set; }

            /// <summary>
            /// Indicates if this is the official master server (Oculus GameLift API).
            /// </summary>
            public bool IsOfficial => GraphUrl == "https://graph.oculus.com";
        }
    }
}