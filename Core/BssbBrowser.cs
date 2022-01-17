using System;
using System.Threading;
using System.Threading.Tasks;
using ServerBrowser.Models.Requests;
using ServerBrowser.Models.Responses;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Core
{
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
            SetLoadingState(true);

            // Calculate pagination offset
            var pageSize = PageData?.Limit ?? DefaultPageSize;
            QueryParams.Offset = (_pageIndex * pageSize);
            
            _log.Info($"Loading page (index={_pageIndex})...");
            
            // Query API (load page)
            try
            {
                PageData = await _apiClient.Browse(QueryParams, _loadingCts!.Token);
            }
            catch (TaskCanceledException ex)
            {
                PageData = null;
            }

            if (PageData is not null)
                _log.Info($"BrowseData received (TotalCount={PageData.Count}, Limit={PageData.Limit}, " +
                          $"LobbiesCount={PageData.Lobbies?.Count ?? 0}, Message={PageData.Message})");
            else
                _log.Info("Received NULL response!");
            
            // Trigger update
            SetLoadingState(false, (PageData == null));
            UpdateEvent?.Invoke(this, EventArgs.Empty);
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
                SetLoadingState(false, true);
                UpdateEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetLoadingState(bool isLoading, bool didError = false)
        {
            _log.Info($"Data loading state set (isLoading={isLoading}, didError={didError})");
            
            IsLoading = isLoading;
            LoadingErrored = didError;
        }
    }
}