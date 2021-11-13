using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.Game.Models;
using ServerBrowser.UI;

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
            
            RegisterProvider(new DiscordCore.DiscordPresenceProvider());
            RegisterProvider(new Steam.SteamPresenceProvider());
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
        
        #region Launcher
        private CancellationTokenSource? _joinCancellationTokenSource;
        
        public async Task JoinFromSecret(string bssbKey)
        {
            // Check connection state
            var mpActivity = GameStateManager.Activity;

            if (!mpActivity.InOnlineMenu || mpActivity.IsInMultiplayer)
            {
                Plugin.Log?.Error($"[PresenceManager] Ignoring invite (bssbKey={bssbKey}) - not in online menu!");
                FloatingNotification.Instance.ShowMessage(
                    title: "Invite error",
                    message: "To accept multiplayer invites, you must be in the Online menu.",
                    FloatingNotification.NotificationStyle.Cerise
                );
                return;
            }
            
            // Cancel any previous joins
            _joinCancellationTokenSource?.Cancel();
            _joinCancellationTokenSource?.Dispose();
            _joinCancellationTokenSource = new CancellationTokenSource();
            
            // UI notice
            FloatingNotification.Instance.ShowMessage(
                title: "Accepting invitation",
                message: "Please wait, trying to join lobby from invitation...",
                FloatingNotification.NotificationStyle.Cerise
            );
            
            // Presence secret is actually the key of the game on BSSB - query game data
            Plugin.Log?.Info($"[PresenceManager] Trying to join game from Rich Presence (bssbKey={bssbKey})");
            
            var gameData = await BSSBMasterAPI.BrowseDetail(bssbKey, _joinCancellationTokenSource.Token);

            if (gameData is null || !gameData.CanJoin)
            {
                Plugin.Log?.Error($"[PresenceManager] Join failed; could not fetch game data (bssbKey={bssbKey})");
                return;
            }

            HMMainThreadDispatcher.instance.Enqueue(() => MpConnect.Join(gameData));
        }
        #endregion
    }
}