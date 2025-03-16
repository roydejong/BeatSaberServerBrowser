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

namespace ServerBrowser.UI.Utils
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CoverArtLoader : MonoBehaviour, IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly IHttpService _httpService = null!;

        private readonly Dictionary<string, Sprite> _coverArtCache;

        public CoverArtLoader()
        {
            _coverArtCache = new(10);
        }

        public void Dispose()
        {
            UnloadCache();
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

        public async Task<Sprite?> FetchCoverArtAsync(CoverArtRequest request)
        {
            try
            {
                if (request.LevelId == null)
                    return null;

                // Check if cover art is already in cache
                var fromCache = ResolveCache(request.LevelId, null);
                if (fromCache != null)
                    return fromCache;

                // Check if cover art is available locally
                // TODO Get local cover art if we can

                // If we have a URL, try to download cover art (BSSB mirror or BeatSaver CDN) 
                if (request.CoverArtUrl != null)
                {
                    var remoteCoverArtBytes = await TryGetRemoteCoverArtBytes(request.LevelId,
                        request.CoverArtUrl, request.CancellationToken);

                    if (remoteCoverArtBytes != null)
                    {
                        var sprite = Sprites.LoadSpriteRaw(remoteCoverArtBytes,
                            spriteMeshType: SpriteMeshType.FullRect);

                        if (sprite != null)
                        {
                            sprite = ResolveCache(request.LevelId, sprite);
                        }
                        else
                        {
                            _log.Warn($"Sprite load error for remote cover art " +
                                      $"(LevelId={request.LevelId}, CoverArtUrl={request.CoverArtUrl})");
                        }

                        return sprite;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _log.Error($"CoverArtLoader failed with exception: {ex}");
            }

            return null;
        }

        private async Task<byte[]?> TryGetRemoteCoverArtBytes(string levelId, string imageUrl,
            CancellationToken token)
        {
            try
            {
                var coverResponse = await _httpService.GetAsync(imageUrl, cancellationToken: token);
                var coverBytes = await coverResponse.ReadAsByteArrayAsync();
                return coverBytes;
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

            public CoverArtRequest(BssbLevel? levelInfo, CancellationToken cancellationToken)
            {
                LevelId = levelInfo?.LevelId;
                CoverArtUrl = levelInfo?.CoverArtUrl;
                CancellationToken = cancellationToken;
            }

            public CoverArtRequest(BssbServer serverInfo, CancellationToken cancellationToken)
                : this(serverInfo.Level, cancellationToken)
            {
            }
        }
    }
}