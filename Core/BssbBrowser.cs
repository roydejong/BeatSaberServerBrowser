using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ServerBrowser.Models;
using ServerBrowser.Models.Requests;
using ServerBrowser.Models.Responses;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Utility for browsing paginated lobbies on the Server Browser API.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbBrowser
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbApiClient _apiClient = null!;
        
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
        
        #endregion

        #region Refresh data
        
        private void ProcessApiResponse(BrowseResponse apiResponse)
        {
            MessageOfTheDay = apiResponse.MessageOfTheDay;

            if (apiResponse.Servers == null)
                return;

            // Add or update servers, index missing servers
            var keysMissing = new HashSet<string>(AllServers.Keys);
            
            foreach (var server in apiResponse.Servers)
            {
                AllServers[server.Key] = server;
                keysMissing.Remove(server.Key);
            }
            
            // Remove missing servers
            foreach (var key in keysMissing)
            {
                AllServers.Remove(key);
            }
            
            // TODO Sort, probably
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
    }
}