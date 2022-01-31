using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ServerBrowser.Assets;
using ServerBrowser.Models;
using SiraUtil.Logging;
using SiraUtil.Web;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CoverArtLoader : MonoBehaviour
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly IHttpService _httpService = null!;

        private Dictionary<string, Sprite> _coverArtCache;

        public CoverArtLoader()
        {
            _coverArtCache = new(10);
        }

        #region Cache / resource management

        public void UnloadCache()
        {
            foreach (var sprite in _coverArtCache.Values)
                UnloadSprite(sprite);
            _coverArtCache.Clear();
        }

        private Sprite? ResolveCache(string levelId, Sprite? newSprite)
        {
            // Due to async requests, we may already have an item in cache, prevent duplication and use original
            if (_coverArtCache.ContainsKey(levelId) && _coverArtCache[levelId] != newSprite
                                                    && _coverArtCache[levelId].texture != null)
            {
                if (newSprite != null)
                {
                    _log.Warn($"Prevented dupe cache - levelId={levelId}"); // is this actually needed?
                    UnloadSprite(newSprite);
                }

                return _coverArtCache[levelId];
            }

            // Otherwise, just add the provided sprite to cache
            if (newSprite != null)
            {
                newSprite.name = levelId;
                _coverArtCache[levelId] = newSprite;
                return newSprite;
            }

            // Fallback: no cache hit, no new sprite provided
            return null;
        }

        private void UnloadSprite(Sprite sprite) => Destroy(sprite.texture);

        #endregion

        public async Task<Sprite?> FetchCoverArt(CoverArtRequest request)
        {
            if (request.LevelId != null)
            {
                // Check if cover art is already in cache
                var fromCache = ResolveCache(request.LevelId, null);
                if (fromCache != null)
                    return fromCache;

                // Check if cover art is available locally
                var localCoverArt = await TryGetLocalCoverArtSprite(request.LevelId, request.CancellationToken);

                if (localCoverArt != null)
                    return localCoverArt;

                // If we have a URL, try to download cover art (BSSB mirror or BeatSaver CDN) 
                if (request.CoverArtUrl != null)
                {
                    var remoteCoverArt = await TryGetRemoteCoverArtSprite(request.LevelId, request.CoverArtUrl,
                        request.CancellationToken);

                    if (remoteCoverArt != null)
                        return remoteCoverArt;
                }
            }

            return null;
        }

        private async Task<Sprite?> TryGetLocalCoverArtSprite(string levelId, CancellationToken token)
        {
            try
            {
                var level = SongCore.Loader.GetLevelById(levelId);

                if (level == null)
                    return null;

                // Official level, or installed custom level found
                return ResolveCache(levelId, await level.GetCoverImageAsync(token));
            }
            catch (TaskCanceledException)
            {
                // Cancellation token was cancelled
            }
            catch (Exception ex)
            {
                _log.Warn($"Exception while trying to get local cover art (levelId={levelId}): {ex}");
            }

            return null;
        }

        private async Task<Sprite?> TryGetRemoteCoverArtSprite(string levelId, string imageUrl,
            CancellationToken token)
        {
            try
            {
                var coverResponse = await _httpService.GetAsync(imageUrl, cancellationToken: token);
                var coverBytes = await coverResponse.ReadAsByteArrayAsync();

                var sprite = Sprites.LoadSpriteRaw(coverBytes);

                if (sprite == null)
                {
                    _log.Warn($"Sprite load error for remote cover art (levelId={levelId}, imageUrl={imageUrl})");
                    return null;
                }

                return ResolveCache(levelId, sprite);
            }
            catch (TaskCanceledException)
            {
                // Cancellation token was cancelled
            }
            catch (Exception ex)
            {
                _log.Warn($"Exception while trying to get remote cover art (levelId={levelId}, " +
                          $"imageUrl={imageUrl}): {ex}");
            }

            return null;
        }

        public class CoverArtRequest
        {
            public readonly string? LevelId;
            public readonly string? CoverArtUrl;
            public readonly CancellationToken CancellationToken;

            public CoverArtRequest(BssbServer serverInfo, CancellationToken cancellationToken)
            {
                LevelId = serverInfo.Level?.LevelId;
                CoverArtUrl = serverInfo.CoverArtUrl ?? serverInfo.Level?.CoverArtUrl;
                CancellationToken = cancellationToken;
            }
        }
    }
}