using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ServerBrowser.Models;
using ServerBrowser.Models.Requests;
using ServerBrowser.Models.Responses;
using ServerBrowser.Network.Discovery;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Utility for browsing paginated lobbies on the Server Browser API.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbBrowser : IInitializable, IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbApiClient _apiClient = null!;
        [Inject] private readonly DiscoveryClient _discoveryClient = null!;
        
        /// <summary>
        /// This event is raised when loading a page has finished or failed.
        /// </summary>
        public event EventHandler? UpdateEvent;

        public BrowseQueryParams QueryParams = new();
        public readonly Dictionary<string, BssbServer> AllServers = new();
        public string? MessageOfTheDay { get; private set; } = null;

        public bool IsLoading { get; private set; }
        public bool ApiRequestFailed { get; private set; }

        private CancellationTokenSource? _loadingCts;
        private readonly Dictionary<string, float> _discoveryResponseAges = new();
        
        private const int DiscoveryTimeoutSeconds = 30; // discovery packets are sent every 5 secs

        #region Init

        public void Initialize()
        {
            _discoveryClient.ResponseReceived += HandleDiscoveryResponse;
        }
        
        public void Dispose()
        {
            _discoveryClient.Dispose();
            _discoveryClient.ResponseReceived -= HandleDiscoveryResponse;
        }

        #endregion
        
        #region Refresh API
        
        public async Task ResetRefresh()
        {
            CancelLoading();
            await Refresh();
        }

        public async Task Refresh()
        {
            CancelLoading();
            TriggerUpdate(true);

            // Query API (load page)
            BrowseResponse? apiResult = null;

            try
            {
                apiResult = await _apiClient.Browse(QueryParams, _loadingCts!.Token);

                if (apiResult is null)
                    _log.Warn($"Browser API request failed (request error, or invalid response)");
                else if (apiResult.Servers == null)
                    _log.Warn($"Browser API sent null server list");
                else
                    ProcessApiResponse(apiResult);
            }
            catch (TaskCanceledException)
            {
                _log.Info($"Browser API request cancelled");
            }

            // Trigger update
            TriggerUpdate(false, apiResult == null);
        }

        public void CancelLoading()
        {
            _loadingCts?.Cancel();
            _loadingCts?.Dispose();
            _loadingCts = new();
        }

        public void EnableDiscovery()
        {
            _discoveryClient.StartBroadcast();
        }

        public void DisableDiscovery()
        {
            _discoveryClient.StopBroadcast();
        }
        
        #endregion

        #region Refresh data handlers
        
        private void ProcessApiResponse(BrowseResponse apiResponse)
        {
            MessageOfTheDay = apiResponse.MessageOfTheDay;

            if (apiResponse.Servers == null)
                return;
            
            // Remove stale discovery responses
            var localDiscoveryKeys = new HashSet<string>(_discoveryResponseAges.Keys);
            
            foreach (var discoveryKey in localDiscoveryKeys)
            {
                var timeDiff = Time.realtimeSinceStartup - _discoveryResponseAges[discoveryKey];
                if (timeDiff < DiscoveryTimeoutSeconds)
                    continue;
                
                // Timed out
                AllServers.Remove(discoveryKey);
                _discoveryResponseAges.Remove(discoveryKey);
            }

            // Add or update servers, index missing servers
            var keysMissing = new HashSet<string>(AllServers.Keys);
            
            foreach (var server in apiResponse.Servers)
            {
                AllServers[server.Key] = server;
                keysMissing.Remove(server.Key);
            }
            
            // Remove servers not in API response
            foreach (var key in keysMissing)
            {
                if (localDiscoveryKeys.Contains(key))
                    // Don't remove
                    continue;
                    
                AllServers.Remove(key);
            }
        }
        
        private void HandleDiscoveryResponse(DiscoveryResponsePacket response, IPEndPoint source)
        {
            var serverData = response.ToServerData(source);
            
            AllServers[serverData.Key] = serverData;
            _discoveryResponseAges[serverData.Key] = Time.realtimeSinceStartup;
                
            TriggerUpdate(IsLoading);
        }
        
        #endregion

        #region Events

        private void TriggerUpdate(bool isLoading, bool apiRequestFailed = false)
        {
            IsLoading = isLoading;
            ApiRequestFailed = apiRequestFailed;

            UpdateEvent?.Invoke(this, EventArgs.Empty);
        }
        
        #endregion

        #region Sort helper

        public List<BssbServer> GetAllServersSorted()
        {
            return AllServers.Values
                // Primary sort: preference, our guess on how likely the player wants to see/join it
                .OrderByDescending(server => server.PreferentialSortScore)
                // Secondary sort: prefer newer lobbies
                .ThenByDescending(server => server.ReadOnlyFirstSeen)
                .ToList();
        }
 
        #endregion
    }
}