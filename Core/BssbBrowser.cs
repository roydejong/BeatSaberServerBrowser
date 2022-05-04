using System;
using System.Threading;
using System.Threading.Tasks;
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
        public const int DefaultPageSize = 6;
        
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbApiClient _apiClient = null!;

        public readonly BrowseQueryParams QueryParams = new();
        public BrowseResponse? PageData { get; private set; }

        /// <summary>
        /// This event is raised when loading a page has finished or failed.
        /// </summary>
        public event EventHandler? UpdateEvent;
        
        private CancellationTokenSource? _loadingCts;
        private int _pageIndex;

        public bool IsLoading { get; private set; }
        public bool LoadingErrored { get; private set; }

        public async Task Reset()
        {
            CancelLoading();
            _pageIndex = 0;
            await Refresh();
        }

        public async Task Refresh()
        {
            CancelLoading();
            TriggerUpdate(true);

            // Calculate pagination offset
            var pageSize = PageData?.PageSize ?? DefaultPageSize;
            QueryParams.Offset = (_pageIndex * pageSize);
            
            // Query API (load page)
            try
            {
                PageData = await _apiClient.Browse(QueryParams, _loadingCts!.Token);
            }
            catch (TaskCanceledException)
            {
                PageData = null;
            }

            if (PageData is not null)
                _log.Debug($"BrowseData loaded page (Index={_pageIndex}, TotalCount={PageData.TotalResultCount}, " +
                          $"Limit={PageData.PageSize}, LobbiesCount={PageData.Servers?.Count ?? 0}, " +
                          $"MOTD={PageData.MessageOfTheDay})");
            else
                _log.Error($"BrowseData page load failed - received null response (Index={_pageIndex})");
            
            // Trigger update
            TriggerUpdate(false, (PageData == null));
        }

        public async Task PageUp()
        {
            if (_pageIndex <= 0)
                return;
            
            _pageIndex--;
            await Refresh();
        }

        public async Task PageDown()
        {
            _pageIndex++;
            await Refresh();
        }

        public void CancelLoading()
        {
            _loadingCts?.Cancel();
            _loadingCts?.Dispose();
            
            _loadingCts = new();

            if (IsLoading)
            {
                TriggerUpdate(false, true);
            }
        }

        private void TriggerUpdate(bool isLoading, bool didError = false)
        {
            IsLoading = isLoading;
            LoadingErrored = didError;
            
            UpdateEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}