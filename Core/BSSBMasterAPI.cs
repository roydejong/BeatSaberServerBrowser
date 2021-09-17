using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ServerBrowser.Core.Responses;
using ServerBrowser.Game;

namespace ServerBrowser.Core
{
    public static class BSSBMasterAPI
    {
        #region Shared/HTTP
        private const string BaseURL = "https://bssb.app/api/v1";

        private static async Task<HttpResponseMessage?> PerformWebRequest(string method, string endpoint,
            string? json = null, CancellationToken? cancellationToken = null)
        {
            var targetUrl = BaseURL + endpoint;
            
            Plugin.Log?.Debug($"[BSSBMasterAPI] {method} {targetUrl} {json}");

            try
            {
                HttpResponseMessage response;

                if (method == "GET")
                {
                    response = await Plugin.HttpClient.GetAsync(targetUrl).ConfigureAwait(false);
                }
                else if (method == "POST")
                {
                    if (String.IsNullOrEmpty(json))
                    {
                        if (cancellationToken is null)
                            response = await Plugin.HttpClient.PostAsync(targetUrl, null).ConfigureAwait(false);
                        else
                            response = await Plugin.HttpClient.PostAsync(targetUrl, null, cancellationToken.Value).ConfigureAwait(false);
                    }
                    else
                    {
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        
                        if (cancellationToken is null)
                            response = await Plugin.HttpClient.PostAsync(targetUrl, content).ConfigureAwait(false);
                        else
                            response = await Plugin.HttpClient.PostAsync(targetUrl, content, cancellationToken.Value).ConfigureAwait(false);
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid request method for the Master Server API: {method}");
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Expected HTTP 200 OK, got HTTP {response.StatusCode}");
                }

                Plugin.Log?.Debug($"[BSSBMasterAPI] ✔ 200 OK: {method} {targetUrl}");
                return response;
            }
            catch (TaskCanceledException)
            {
                Plugin.Log?.Warn($"[BSSBMasterAPI] Request task canceled: {method} {targetUrl}");
                return null;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"[BSSBMasterAPI] Request error: {method} {targetUrl} → {ex}");
                return null;
            }
        }
        #endregion

        public static async Task<AnnounceResult> Announce(HostedGameData announce)
        {
            var payload = announce.ToJson();
            
            Plugin.Log?.Info($"Sending host announcement (OwnerId={announce.OwnerId}, " +
                             $"HostSecret={announce.HostSecret}, ServerCode={announce.ServerCode}, " +
                             $"Endpoint={announce.Endpoint}, ServerType={announce.ServerType})...");

            var response = await PerformWebRequest("POST", "/announce", payload);

            if (response is not null)
            {
                try
                {
                    var contentStr = await response.Content.ReadAsStringAsync();
                    return AnnounceResult.FromJson(contentStr);
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Warn($"Error parsing Announce response: {ex}");
                }
            }
            
            return new AnnounceResult() { Success = false };
        }

        public static async Task<bool> UnAnnounce(AnnounceState announceState)
        {
            Plugin.Log?.Info($"Cancelling host announcement (ServerCode={announceState.ServerCode}, " +
                             $"OwnerId={announceState.OwnerId}, HostSecret={announceState.HostSecret})");

            var queryString = HttpUtility.ParseQueryString("");
            queryString.Add("serverCode", announceState.ServerCode);
            queryString.Add("ownerId", announceState.OwnerId);
            queryString.Add("hostSecret", announceState.HostSecret);

            return await PerformWebRequest("POST", $"/unannounce?{queryString}") != null;
        }

        public static async Task<BrowseResult?> Browse(int offset, HostedGameFilters filters)
        {
            var queryString = HttpUtility.ParseQueryString("");

            if (MpLocalPlayer.Platform.HasValue)
                queryString.Add("platform", MpLocalPlayer.PlatformId);

            if (offset > 0)
                queryString.Add("offset", offset.ToString());
            
            if (!MpSession.GetLocalPlayerHasMultiplayerExtensions())
                queryString.Add("vanilla", "1");
            
            if (!String.IsNullOrEmpty(filters.TextSearch))
                queryString.Add("query", filters.TextSearch);
            
            if (filters.HideFullGames)
                queryString.Add("filterFull", "1");
            
            if (filters.HideInProgressGames)
                queryString.Add("filterInProgress", "1");
            
            if (filters.HideModdedGames)
                queryString.Add("filterModded", "1");

            var response = await PerformWebRequest("GET", $"/browse?{queryString}");

            if (response is not null)
            {
                try
                {
                    var contentStr = await response.Content.ReadAsStringAsync();
                    return BrowseResult.FromJson(contentStr);
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Warn($"Error parsing Browse response: {ex}");
                }
            }
            
            return null;
        }
        
        public static async Task<HostedGameData?> BrowseDetail(string key, CancellationToken? cancellationToken = null)
        {
            var response = await PerformWebRequest("GET", $"/browse/{key}", null, cancellationToken);

            if (response == null)
            {
                Plugin.Log?.Warn($"Browse failed, did not get a valid response");
                return null;
            }

            try
            {
                var contentStr = await response.Content.ReadAsStringAsync();
                return HostedGameData.FromJson(contentStr);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Error parsing browser response: {ex}");
                return null;
            }
        }
    }
}

