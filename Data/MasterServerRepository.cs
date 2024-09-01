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

        private readonly Dictionary<string, MasterServerInfo> _masterServers = new();
        private MasterServerInfo? _selectedServer = null;

        public IReadOnlyList<MasterServerInfo> All
        {
            get
            {
                var list = _masterServers.Values.ToList();
                return list.Prepend(OfficialServer).ToList();
            }
        }

        public MasterServerInfo SelectedMasterServer
        {
            get
            {
                if (_selectedServer != null)
                    return _selectedServer;
                return OfficialServer;
            }
            set
            {
                _log.Info($"Select master server: {value.DisplayName} ({value.GraphUrl})");
                _selectedServer = !value.IsOfficial ? value : null;
                WriteConfig();
                MasterServerSelectionChangedEvent?.Invoke(SelectedMasterServer);
            }
        }
        
        public static readonly MasterServerInfo OfficialServer = new()
        {
            GraphUrl = "https://graph.oculus.com",
            StatusUrl = "https://graph.oculus.com/beat_saber_multiplayer_status",
            Name = "Official Servers",
            Description = "Beat Saber Official Multiplayer (OST/DLC Only)",
            MaxPlayers = 5,
            UseSsl = true
        };

        public event Action<MasterServerInfo>? MasterServerSelectionChangedEvent;

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
            // Apply previously selected master server
            if (_bssbConfig.SelectedMasterServer != null)
                if (_masterServers.TryGetValue(_bssbConfig.SelectedMasterServer, out var selectedServer))
                    _selectedServer = selectedServer;

            // Load list; assume config is latest; if we had updates we would have written them back to the config
            foreach (var cfgMaster in _bssbConfig.MasterServers)
            {
                if (cfgMaster.IsOfficial)
                    continue;

                cfgMaster.LastUpdated ??= DateTime.Now;

                _masterServers[cfgMaster.GraphUrl] = cfgMaster;
            }
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

                    if (remoteServer.IsOfficial)
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
            public string GraphUrl { get; init; } = null!;

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
            [JsonProperty("lastUpdated")]
            public DateTime? LastUpdated { get; set; }

            /// <summary>
            /// Modded feature: per-player modifiers.
            /// </summary>
            [JsonProperty("supportsPpModifiers")]
            public bool SupportsPpModifiers { get; set; }

            /// <summary>
            /// Modded feature: per-player difficulties.
            /// </summary>
            [JsonProperty("supportsPpDifficulties")]
            public bool SupportsPpDifficulties { get; set; }

            /// <summary>
            /// Modded feature: per-player maps.
            /// </summary>
            [JsonProperty("supportsPpMaps")]
            public bool SupportsPpMaps { get; set; }

            /// <summary>
            /// Indicates if this is the official master server (Oculus GameLift API).
            /// </summary>
            [JsonIgnore]
            public bool IsOfficial => GraphUrl == "https://graph.oculus.com";

            [JsonIgnore]
            public string DisplayName => Name ?? GraphUrlNoProtocolNoPort;
            
            /// <summary>
            /// Gets the graph URL without the protocol:// prefix.
            /// </summary>
            [JsonIgnore]
            public string GraphUrlNoProtocol => GraphUrl
                .Replace("https://", "")
                .Replace("http://", "")
                .Trim('/');
            
            [JsonIgnore]
            public string GraphUrlNoProtocolNoPort => GraphUrlNoProtocol
                .Split(':').First();

            [JsonIgnore]
            public ServerRepository.ConnectionMethod ConnectionMethod
            {
                get
                {
                    if (IsOfficial)
                        return ServerRepository.ConnectionMethod.GameLiftOfficial;
                    else if (UseSsl)
                        return ServerRepository.ConnectionMethod.GameLiftModdedSsl;
                    else
                        return ServerRepository.ConnectionMethod.GameLiftModded;
                }
            }
        }
    }
}