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

            Plugin.Log?.Info($"{method} {targetUrl}");

            try
            {
                HttpResponseMessage response;

                switch (method)
                {
                    case "GET":
                        response = await client.GetAsync(targetUrl);
                        break;
                    case "POST":
                        response = await client.PostAsync(targetUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                        break;
                    case "DELETE":
                        response = await client.DeleteAsync(targetUrl);
                        break;
                    default:
                        throw new ArgumentException($"Invalid request method for the Master Server API: {method}");
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Expected HTTP 200 OK, got HTTP {response.StatusCode}");
                }

                Plugin.Log?.Info($"✔ 200 OK: {method} {targetUrl}");
                return response;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"⚠ Request error: {method} {targetUrl} → {ex}");
                return null;
            }
        }
        #endregion

        public static async Task<bool> SendAnnounce(LobbyAnnounceInfo announce)
        {
            return await PerformWebRequest("POST", "/announce", "abc") != null;
        }

        public static async Task<bool> SendDeleteAnnounce(LobbyAnnounceInfo announce)
        {
            return await PerformWebRequest("DELETE", $"/announce?ownerId={announce.OwnerId}&serverCode={announce.ServerCode}") != null;
        }
    }
}
