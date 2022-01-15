using System;
using System.Reflection;
using System.Threading.Tasks;
using ServerBrowser.Models;
using ServerBrowser.Models.Responses;
using SiraUtil.Logging;
using SiraUtil.Web;
using Zenject;

namespace ServerBrowser.Core
{
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

        [Inject] private readonly SiraLog _log;
        [Inject] private readonly IHttpService _httpService;
        
        public void Initialize()
        {
            _httpService.BaseURL = "https://bssb.app/api";
            _httpService.UserAgent = UserAgent;
            _httpService.Headers.Add("X-BSSB", "hello");
            
            _log.Info($"Initialized API client ({UserAgent})");
        }

        public async Task<AnnounceResponse?> Announce(object announceData)
        {
            try
            {
                var response = await _httpService.PostAsync($"/v1/announce");
                var responseBody = await response.ReadAsStringAsync();
                return AnnounceResponse.FromJson<AnnounceResponse>(responseBody);
            }
            catch (Exception ex)
            {
                _log.Error($"Announce error: {ex.Message}");
                return null;
            }
        }

        public async Task<UnAnnounceResponse?> UnAnnounce()
        {
            try
            {
                var response = await _httpService.PostAsync($"/v1/unannounce");
                var responseBody = await response.ReadAsStringAsync();
                return UnAnnounceResponse.FromJson(responseBody);
            }
            catch (Exception ex)
            {
                _log.Error($"UnAnnounce error: {ex.Message}");
                return null;
            }
        }

        public async Task<BrowseResponse?> Browse(BrowseQueryParams queryParams)
        {
            try
            {
                var response = await _httpService.GetAsync($"/v1/browse?{queryParams.ToQueryString()}");
                var responseBody = await response.ReadAsStringAsync();
                return BrowseResponse.FromJson(responseBody);
            }
            catch (Exception ex)
            {
                _log.Error($"Browse error: {ex.Message}");
                return null;
            }
        }

        public async Task<BssbServerDetail?> BrowseDetail(string key)
        {
            try
            {
                var response = await _httpService.GetAsync($"/v1/browse/{key}");
                var responseBody = await response.ReadAsStringAsync();
                // TODO return BssbServerDetail.FromJson(responseBody);
            }
            catch (Exception ex)
            {
                _log.Error($"BrowseDetail error: {ex.Message}");
            }
            return null;
        }
    }
}