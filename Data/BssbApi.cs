using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerBrowser.Requests;
using ServerBrowser.Responses;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Data
{
    public class BssbApi : IInitializable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbConfig _config = null!;
        
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
        
        private readonly HttpClient _httpClient;

        public BssbApi()
        {
            _httpClient = new();
        }
        
        public void Initialize()
        {
            _httpClient.BaseAddress = new Uri(_config.ApiServerUrl);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            _httpClient.DefaultRequestHeaders.Add("X-BSSB", "✔");

            _log.Debug($"Initialized API client [{_config.ApiServerUrl}, {UserAgent}]");
        }
        
        private async Task<TResponse?> Post<TRequest, TResponse>(string path, TRequest request)
        {
            try
            {
                var requestJson = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                
                _log.Info($"[Request] POST {path}");
                if (request is not BssbLoginRequest) // never log user auth tokens
                    _log.Debug(requestJson);

                var response = await _httpClient.PostAsync(path, requestContent);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<TResponse>(responseBody);
            }
            catch (Exception ex)
            {
                _log.Error($"Request failed: POST {path}: {ex.Message}");
                _log.Debug(ex.StackTrace);
                return default;
            }
        }
        
        public async Task<BssbLoginResponse?> SendLoginRequest(BssbLoginRequest request)
            => await Post<BssbLoginRequest, BssbLoginResponse>("/api/v2/login", request);
        
        public async Task<BssbBrowseResponse?> SendBrowseRequest()
            => await Post<BssbEmptyRequest, BssbBrowseResponse>("/api/v2/browse", new BssbEmptyRequest());
    }
}