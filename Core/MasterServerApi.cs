using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ServerBrowser.Core
{
    public static class MasterServerAPI
    {
        #region Shared/HTTP
        private const string BASE_URL = "https://bs-lobby-master.roydejong.net/api/v1";

        private static async Task<HttpResponseMessage> PerformWebRequest(string method, string endpoint, string json = null)
        {
            var targetUrl = BASE_URL + endpoint;
            Plugin.Log?.Debug($"{method} {targetUrl} {json}");

            try
            {
                HttpResponseMessage response;

                switch (method)
                {
                    case "GET":
                        response = await Plugin.HttpClient.GetAsync(targetUrl).ConfigureAwait(false);
                        break;
                    case "POST":
                        if (String.IsNullOrEmpty(json))
                        {
                            response = await Plugin.HttpClient.PostAsync(targetUrl, null).ConfigureAwait(false);
                        }
                        else
                        {
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            response = await Plugin.HttpClient.PostAsync(targetUrl, content).ConfigureAwait(false);
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
            catch (TaskCanceledException ex)
            {
                return null;
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

        public static async Task<bool> Announce(HostedGameData announce)
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

        public static async Task<bool> UnAnnounce(HostedGameData announce)
        {
            Plugin.Log?.Info($"Cancelling host announcement: {announce.GameName}, {announce.ServerCode}");

            _lastPayloadSent = null;

            var responseOk = await PerformWebRequest("POST", $"/unannounce?serverCode={announce.ServerCode}&ownerId={announce.OwnerId}") != null;
            return responseOk;
        }

        public static async Task<ServerBrowseResult> Browse(int offset, string searchQuery)
        {
            var searchQueryEncoded = "";

            if (String.IsNullOrEmpty(searchQuery))
            {
                Plugin.Log?.Info($"Requesting lobbies from server (offset {offset})");
            }
            else
            {
                Plugin.Log?.Info($"Requesting lobbies from server, searching for \"{searchQuery}\" (offset {offset})");
                searchQueryEncoded = HttpUtility.UrlEncode(searchQuery);
            }

            var response = await PerformWebRequest("GET", $"/browse?platform={Plugin.PlatformId}&offset={offset}&query={searchQueryEncoded}");
            var contentStr = await response.Content.ReadAsStringAsync();     

            try
            {
                return ServerBrowseResult.FromJson(contentStr);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Error while parsing browse result: {ex}");
                return null;
            }
        }
    }
}
