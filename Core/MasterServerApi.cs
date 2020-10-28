using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LobbyBrowserMod.Core
{
    public static class MasterServerApi
    {
        #region Shared/HTTP
        private const string BASE_URL = "https://bs-lobby-master.roydejong.net/api/v1";

        private static string UserAgent
        {
            get
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                return $"LobbyBrowserMod/{assemblyVersion}";
            }
        }

        private static async Task<HttpWebResponse> PerformWebRequest(string method, string endpoint, string requestBody = null)
        {
            var targetUrl = BASE_URL + endpoint;

            Plugin.Log?.Info($"{method} {targetUrl}");

            try
            {
                var request = WebRequest.CreateHttp(targetUrl);
                request.Method = method;
                request.UserAgent = UserAgent;
                request.Headers["X-BSLBM"] = ":-)";

                if (!String.IsNullOrEmpty(requestBody))
                {
                    if (method == "POST" || method == "DELETE")
                    {
                        request.ContentType = "application/json; charset=utf-8";
                    }
                    else
                    {
                        throw new InvalidOperationException("Request body is only allowed on POST and DELETE requests");
                    }

                    var stream = await request.GetRequestStreamAsync();
                    var bodyBytes = Encoding.UTF8.GetBytes(requestBody);
                    await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
                }

                var response = (HttpWebResponse)await request.GetResponseAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Expected HTTP 200 OK, got {response.StatusCode} ({response.StatusDescription})");
                }

                Plugin.Log?.Info($"✔ 200 OK: {method} {targetUrl}");
                return response;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"⚠ Request error: {method} {targetUrl} → {ex.Message}");
                return null;
            }
        }
        #endregion

        public static async Task<bool> SendAnnounce(LobbyAnnounceInfo announce)
        {
            return await PerformWebRequest("POST", "/announce") != null;
        }

        public static async Task<bool> SendDeleteAnnounce(LobbyAnnounceInfo announce)
        {
            return await PerformWebRequest("DELETE", "/announce") != null;
        }
    }
}
