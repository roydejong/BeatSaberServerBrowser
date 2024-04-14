using System;
using System.Collections.Generic;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using JetBrains.Annotations;
using ServerBrowser.Data;

namespace ServerBrowser.Models
{
    [UsedImplicitly]
    public class BssbConfig
    {
        /// <summary>
        /// The base URL for the BSSB API.
        /// </summary>
        public virtual string BssbApiUrl { get; set; } = "https://bssb.app/";
        
        /// <summary>
        /// Toggles whether LAN discovery of servers is enabled.
        /// </summary>
        public virtual bool EnableLocalNetworkDiscovery { get; set; } = true;
        
        /// <summary>
        /// A list of master servers that can be selected by the user.
        /// </summary>
        [NonNullable, UseConverter(typeof(CollectionConverter<MasterServerRepository.MasterServerInfo,
             List<MasterServerRepository.MasterServerInfo?>>))]
        public virtual List<MasterServerRepository.MasterServerInfo> MasterServers { get; set; } = new();

        /// <summary>
        /// Graph URL for the selected master server.
        /// If set to null, or if no matching master server info is known, will use official server.
        /// </summary>
        public virtual string? SelectedMasterServer { get; set; } = null;

        /// <summary>
        /// Controls whether the master server list should be populated / updated from the BSSB API.
        /// </summary>
        public virtual bool RemoteUpdateMasterServerList { get; set; } = true;
        
        /// <summary>
        /// The version of the privacy disclaimer that the user has accepted.
        /// </summary>
        public virtual uint AcceptedPrivacyDisclaimerVersion { get; set; } = 0;

        /// <summary>
        /// Toggle state: whether to announce the server to the BSSB API.
        /// </summary>
        public virtual bool ToggleAnnounce { get; set; } = true;
        
        /// <summary>
        /// Toggle state: per-player modifiers.
        /// </summary>
        public virtual bool TogglePpModifiers { get; set; } = false;
        
        /// <summary>
        /// Toggle state: per-player difficulties.
        /// </summary>
        public virtual bool TogglePpDifficulties { get; set; } = false;
        
        /// <summary>
        /// Toggle state: per-player maps.
        /// </summary>
        public virtual bool TogglePpMaps { get; set; } = false;
        
        internal bool AnyPrivacyDisclaimerAccepted => AcceptedPrivacyDisclaimerVersion > 0;

        public event Action? ReloadedEvent;

        [UsedImplicitly]
        public virtual void OnReload() =>
            ReloadedEvent?.Invoke();
    }
}