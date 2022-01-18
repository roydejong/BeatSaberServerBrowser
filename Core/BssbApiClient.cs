using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
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
    /// HTTP client utility for the Server Browser API.
    /// </summary>
    public class BssbApiClient : IInitializable
    {
        public static string UserAgent
        {
            get
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var assemblyVersionStr = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

                var bsVersion = IPA.Utilities.UnityGame.GameVersion.ToString();

                return $"ServerBrowser/{assemblyVersionStr} (BeatSaber/{bsVersion})";
            }
        }

        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly PluginConfig _config = null!;

        private readonly HttpClient _httpClient;

        public BssbApiClient()
        {
            _httpClient = new();
        }

        public void Initialize()
        {
            _httpClient.BaseAddress = new Uri(_config.ApiServerUrl);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            _httpClient.DefaultRequestHeaders.Add("X-BSSB", "✔");

            _log.Info($"Initialized API client ({UserAgent})");
        }

        public async Task<AnnounceResponse?> Announce(BssbServer announceData)
        {
            try
            {
#if DEBUG
                var rawJson = announceData.ToJson();
                _log.Info($"Sending announce payload: {rawJson}");
                var requestContent = new StringContent(rawJson, Encoding.UTF8, "application/json");
#else
                var requestContent = announceData.ToRequestContent();
#endif

                var response = await _httpClient.PostAsync($"/api/v1/announce", requestContent);
                
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return AnnounceResponse.FromJson<AnnounceResponse>(responseBody);
            }
            catch (Exception ex)
            {
                _log.Warn(ex);
                return null;
            }
        }

        public async Task<UnAnnounceResponse?> UnAnnounce(UnAnnounceParams unannounceData)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/v2/unannounce",
                    unannounceData.ToRequestContent());
                
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return UnAnnounceResponse.FromJson(responseBody);
            }
            catch (Exception ex)
            {
                _log.Warn(ex);
                return null;
            }
        }

        public async Task<BrowseResponse?> Browse(BrowseQueryParams queryParams, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/browse?{queryParams.ToQueryString()}", 
                    cancellationToken);
                
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return BrowseResponse.FromJson(responseBody);
            }
            catch (Exception ex)
            {
                _log.Warn(ex);
                return null;
            }
        }

        public async Task<BssbServerDetail?> BrowseDetail(string key)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/browse/{key}");
                
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return BssbServerDetail.FromJson<BssbServerDetail>(responseBody);
            }
            catch (Exception ex)
            {
                _log.Warn(ex);
            }

            return null;
        }
    }
}