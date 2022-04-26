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
using ThreadPriority = System.Threading.ThreadPriority;

namespace ServerBrowser.UI.Utils
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CoverArtLoader : MonoBehaviour, IInitializable, IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly IHttpService _httpService = null!;

        private readonly Dictionary<string, Sprite> _coverArtCache;
        private readonly Thread _loaderThread;
        private Queue<CoverArtRequest> _pendingRequests;
        private bool _threadKeepAlive;
        private object _threadSyncRoot;

        public CoverArtLoader()
        {
            _coverArtCache = new(10);
            
            _loaderThread = new Thread(__WorkerThread);
            _loaderThread.Name = "BssbCoverArtLoader";
            _loaderThread.Priority = ThreadPriority.BelowNormal;
            
            _pendingRequests = new();
            _threadKeepAlive = false;
            _threadSyncRoot = new();
        }

        #region Worker thread
        public void Initialize()
        {
            _threadKeepAlive = true;
            _loaderThread.Start();
        }

        public void Dispose()
        {
            UnloadCache();

            _threadKeepAlive = false;
            
            lock (_threadSyncRoot)
            {
                Monitor.Pulse(_threadSyncRoot);                
            }
        }

        private async void __WorkerThread()
        {
            while (_threadKeepAlive)
            {
                lock (_threadSyncRoot)
                {
                    Monitor.Wait(_threadSyncRoot);
                }
                
                while (_pendingRequests.Count > 0)
                {
                    try
                    {
                        var request = _pendingRequests.Dequeue();
                        request.CancellationToken.ThrowIfCancellationRequested();

                        if (request.LevelId == null)
                        {
                            request.Callback.Invoke(null);
                            continue;
                        }

                        // Check if cover art is already in cache
                        var fromCache = ResolveCache(request.LevelId, null);

                        if (fromCache != null)
                        {
                            request.Callback.Invoke(fromCache);
                            continue;
                        }

                        // Check if cover art is available locally
                        var localCoverArt = await TryGetLocalCoverArtSprite(request.LevelId, request.CancellationToken);

                        if (localCoverArt != null)
                        {
                            request.Callback.Invoke(localCoverArt);
                            continue;
                        }

                        // If we have a URL, try to download cover art (BSSB mirror or BeatSaver CDN) 
                        if (request.CoverArtUrl != null)
                        {
                            var remoteCoverArtBytes = await TryGetRemoteCoverArtBytes(request.LevelId,
                                request.CoverArtUrl, request.CancellationToken);
                        
                            if (remoteCoverArtBytes != null)
                            {
                                // Sprite creation has to happen on the main thread or Unity will crash
                                HMMainThreadDispatcher.instance.Enqueue(() =>
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
                                    
                                    request.Callback.Invoke(sprite);
                                });
                                continue;
                            }
                        }

                        // Request could not be dealt with
                        HMMainThreadDispatcher.instance.Enqueue(() =>
                        {
                            request.Callback.Invoke(null);
                        });
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        _log.Error($"CoverArtLoader.__WorkerThread: {ex}");
                    }
                }
            }
        }
        #endregion
        
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

        public void FetchCoverArtAsync(CoverArtRequest request)
        {
            _pendingRequests.Enqueue(request);

            lock (_threadSyncRoot)
            {
                Monitor.Pulse(_threadSyncRoot);                
            }
        }

        private async Task<Sprite?> TryGetLocalCoverArtSprite(string levelId, CancellationToken token)
        {
            try
            {
                var level = SongCore.Loader.GetLevelById(levelId);

                if (level == null)
                    return null;

                // Official level, or installed custom level found
                // The game already caches these and disposes of these on its own, so we can't cache them
                return await level.GetCoverImageAsync(token);
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
            public readonly Action<Sprite?> Callback;

            public CoverArtRequest(BssbServer serverInfo, CancellationToken cancellationToken, Action<Sprite?> callback)
            {
                LevelId = serverInfo.Level?.LevelId;
                CoverArtUrl = serverInfo.Level?.CoverArtUrl;
                CancellationToken = cancellationToken;
                Callback = callback;
            }
        }
    }
}