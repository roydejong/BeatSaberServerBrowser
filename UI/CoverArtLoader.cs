using System;
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
    public class CoverArtLoader : MonoBehaviour
    {
        [Inject] private SiraLog _log = null!;
        [Inject] private IHttpService _httpService = null!;

        public async Task<Sprite?> FetchCoverArt(CoverArtRequest request)
        {
            if (request.LevelId == null)
                return null;
                   
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
                var sprite = await level.GetCoverImageAsync(token);
                sprite.name = levelId;
                return sprite;
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

        private async Task<Sprite?> TryGetRemoteCoverArtSprite(string levelId, string imageUrl, CancellationToken token)
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

                sprite.name = levelId;
                return sprite;
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

            public CoverArtRequest(string levelId, CancellationToken cancellationToken)
            {
                LevelId = levelId;
                CoverArtUrl = null;
                CancellationToken = cancellationToken;
            }

            public CoverArtRequest(BssbServer serverInfo, CancellationToken cancellationToken)
            {
                LevelId = serverInfo.Level?.LevelId;
                CoverArtUrl = serverInfo.CoverArtUrl;
                CancellationToken = cancellationToken;
            }
        }
    }
}