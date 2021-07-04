using System;
using System.Collections.Generic;
using System.Linq;
using ServerBrowser.Game;
using ServerBrowser.Game.Models;

namespace ServerBrowser.Presence
{
    /// <summary>
    /// Manages Rich Presence integration for Discord and Steam.
    /// </summary>
    public class PresenceManager
    {
        private bool _started;
        private Dictionary<string, IPresenceProvider> _providers;
        private Dictionary<string, IPresenceProvider> _startedProviders;
        
        public PresenceManager()
        {
            _started = false;
            _providers = new();
            _startedProviders = new();
            
            RegisterProvider(new DiscordCore.PresenceProvider());
        }

        public void RegisterProvider(IPresenceProvider provider)
        {
            var providerId = provider.GetType().FullName;
            
            if (_providers.ContainsKey(providerId))
                return;
            
            if (!provider.GetIsAvailable())
            {
                Plugin.Log?.Warn($"[PresenceManager] Provider disabled or unavailable: {providerId}");
                return;
            }

            _providers.Add(providerId, provider);
        }

        #region Start/Stop
        /// <summary>
        /// Starts all rich presence providers, given the initial activity status.
        /// </summary>
        public void Start(MultiplayerActivity activity)
        {
            if (_started)
                return;
            
            _started = true;
            
            MpEvents.ActivityUpdated += OnMultiplayerActivityUpdated;
            
            Update(activity);
        }

        /// <summary>
        /// Attempts to stop all rich presence providers.
        /// </summary>
        public void Stop()
        {
            if (!_started)
                return;
            
            _started = false;
            
            MpEvents.ActivityUpdated -= OnMultiplayerActivityUpdated;

            // Stop any providers that are running
            foreach (var provider in _startedProviders)
            {
                Plugin.Log?.Info($"[PresenceManager] Stopping rich presence provider: {provider.Key}");

                try
                {
                    provider.Value.Stop();
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Warn($"[PresenceManager] Could not stop provider: {ex}");
                }
            }
            _startedProviders.Clear();
        }
        #endregion

        #region Update/Events
        /// <summary>
        /// Updates all rich presence providers with the latest activity status.
        /// </summary>
        public void Update(MultiplayerActivity activity)
        {
            if (!_started)
                return;
            
            // Start up any pending providers
            foreach (var provider in _providers.ToArray())
            {
                if (!_startedProviders.ContainsKey(provider.Key))
                {
                    try
                    {
                        provider.Value.Start();
                        _startedProviders.Add(provider.Key, provider.Value);
                        Plugin.Log.Info($"[PresenceManager] Started provider: {provider.Key}");
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.Warn($"[PresenceManager] Provider failed to start, disabling: {ex}");
                        _providers.Remove(provider.Key);
                    }
                }
            }
            
            // Perform update
            foreach (var provider in _startedProviders.Values)
            {
                provider.Update(activity);
            }
        }
        
        private void OnMultiplayerActivityUpdated(object sender, MultiplayerActivity activity)
        {
            Update(activity);
        }
        #endregion
    }
}