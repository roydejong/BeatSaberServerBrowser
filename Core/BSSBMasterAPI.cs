﻿using ServerBrowser.Game;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ServerBrowser.Core
{
    public static class BSSBMasterAPI
    {
        #region Shared/HTTP
        private const string BASE_URL = "https://bssb.app/api/v1";

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
            catch (TaskCanceledException)
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

            Plugin.Log?.Info($"Sending host announcement [{announce.Describe()}] for code {announce.ServerCode}");

            _lastPayloadSent = payload;
            _lastSentAt = DateTime.Now;

            var responseOk = await PerformWebRequest("POST", "/announce", payload) != null;

            if (!responseOk)
            {
                // Request failed, allow immediate retry
                _lastPayloadSent = null;
                _lastSentAt = null;
            }

            return responseOk;
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

        public static async Task<ServerBrowseResult> Browse(int offset, HostedGameFilters filters)
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

            if (response == null)
            {
                Plugin.Log?.Warn($"Browse failed, did not get a valid response");
                return null;
            }

            try
            {
                var contentStr = await response.Content.ReadAsStringAsync();
                return ServerBrowseResult.FromJson(contentStr);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Error parsing browser response: {ex}");
                return null;
            }
        }
    }
}

