using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LobbyBrowserMod.Core
{
    public static class MasterServerApi
    {
        #region Shared/HTTP
        private const string BASE_URL = "https://bs-lobby-master.roydejong.net/api/v1";
        private static readonly HttpClient client;

        static MasterServerApi()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            client.DefaultRequestHeaders.Add("X-BSLBM", "✔");
        }

        private static string UserAgent
        {
            get
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                return $"LobbyBrowserMod/{assemblyVersion}";
            }
        }

        private static async Task<HttpResponseMessage> PerformWebRequest(string method, string endpoint, string json = null)
        {
            var targetUrl = BASE_URL + endpoint;

            // Add a GUID to the URL (for some reason requests get stuck if they're not unique???)
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            if (!targetUrl.Contains("?"))
                targetUrl += $"?rnd={guid}";
            else
                targetUrl += $"&rnd={guid}";

            Plugin.Log?.Debug($"{method} {targetUrl} {json}");

            try
            {
                HttpResponseMessage response;

                switch (method)
                {
                    case "GET":
                        response = await client.GetAsync(targetUrl);
                        break;
                    case "POST":
                        if (String.IsNullOrEmpty(json))
                        {
                            response = await client.PostAsync(targetUrl, null);
                        }
                        else
                        {
                            response = await client.PostAsync(targetUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid request method for the Master Server API: {method}");
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Expected HTTP 200 OK, got HTTP {response.StatusCode}");
                }

                Plugin.Log?.Debug($"✔ 200 OK: {method} {targetUrl}");
                return response;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"⚠ Request error: {method} {targetUrl} → {ex}");
                return null;
            }
        }
        #endregion

        private static string _lastPayloadSent = null;
        private static DateTime? _lastSentAt = null;
        private const int ANNOUNCE_INTERVAL_MINS = 1;

        public static async Task<bool> Announce(LobbyAnnounceInfo announce)
        {
            var payload = announce.ToJson();

            var isDupeRequest = (_lastPayloadSent != null && _lastPayloadSent == payload);
            var enoughTimeHasPassed = (_lastSentAt == null
                || (DateTime.Now - _lastSentAt.Value).TotalMinutes >= ANNOUNCE_INTERVAL_MINS);

            if (isDupeRequest && !enoughTimeHasPassed) {
                // An identical payload was already sent out!
                // Do not send a dupe unless enough time has passed
                return true;
            }

            Plugin.Log?.Info($"Sending host announcement [{announce.Describe()}]");

            _lastPayloadSent = payload;
            _lastSentAt = DateTime.Now;

            var responseOk = await PerformWebRequest("POST", "/announce", payload) != null;
            return responseOk;
        }

        public static async Task<bool> UnAnnounce(LobbyAnnounceInfo announce)
        {
            Plugin.Log?.Info($"Cancelling host announcement: {announce.GameName}, {announce.ServerCode}");

            _lastPayloadSent = null;

            var responseOk = await PerformWebRequest("POST", $"/unannounce?serverCode={announce.ServerCode}&ownerId={announce.OwnerId}") != null;
            return responseOk;
        }

        public static async Task<LobbyBrowseResult> Browse(int offset)
        {
            Plugin.Log?.Info($"Requesting lobbies from server (offset {offset})");

            var response = await PerformWebRequest("GET", $"/browse?offset={offset}");
            var contentStr = await response.Content.ReadAsStringAsync();     

            try
            {
                return LobbyBrowseResult.FromJson(contentStr);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Error while parsing browse result: {ex}");
                return null;
            }
        }
    }
}
