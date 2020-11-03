using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBrowser.Utils
{
    public static class BeatSaverHelper
    {
        private static Dictionary<string, string> _hashToCoverURLCache = new Dictionary<string, string>();

        public static async Task<byte[]> FetchCoverArtBytes(string levelId, CancellationToken token)
        {
            // First, extract the hash from the level ID
            var expectedPrefix = "custom_level_";

            if (!levelId.StartsWith(expectedPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("levelId must be a custom level if you want to talk to BeatSaver");
            }

            // Next, query the level data from BeatSaver API
            var levelHash = levelId.Substring(expectedPrefix.Length);
            var coverURL = _hashToCoverURLCache.ContainsKey(levelHash) ? _hashToCoverURLCache[levelHash] : null;

            if (String.IsNullOrEmpty(coverURL))
            {
                // Ask the server for the level's data
                var targetUrl = "https://beatsaver.com/api/maps/by-hash/" + levelHash;

                try
                {
                    var response = await Plugin.HttpClient.GetAsync(targetUrl, token).ConfigureAwait(false);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return null;
                    }

                    var content = JObject.Parse(await response.Content.ReadAsStringAsync());
                    coverURL = content["coverURL"].ToString();

                    if (!String.IsNullOrEmpty(coverURL))
                    {
                        _hashToCoverURLCache.Add(levelHash, coverURL);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"⚠ BeatSaver API request error: {targetUrl} → {ex}");
                    return null;
                }
            }

            if (!String.IsNullOrEmpty(coverURL))
            {
                // Ask the CDN for the cover art
                var targetUrl = "https://beatsaver.com" + coverURL;

                try
                {
                    var coverResponse = await Plugin.HttpClient.GetAsync(targetUrl, token).ConfigureAwait(false);
                    return await coverResponse.Content.ReadAsByteArrayAsync();
                }
                catch (TaskCanceledException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"⚠ BeatSaver CDN request error: {targetUrl} → {ex}");
                    return null;
                }
            }

            return null;
        }
    }
}
