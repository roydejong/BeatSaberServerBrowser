using ServerBrowser.Assets;
using ServerBrowser.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerBrowser.Utils
{
    public static class CoverArtGrabber
    {
        public static async Task<Sprite> GetCoverArtSprite(HostedGameData hostedGame, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(hostedGame.CoverUrl))
            {
                // Server already knows cover art URL, saves us a trip to the beatsaver api
                var downloadedCover = await FetchCoverArtBytesForUrl(hostedGame.CoverUrl, token);

                if (downloadedCover != null)
                {
                    var sprite = Sprites.LoadSpriteRaw(downloadedCover);
                    sprite.name = hostedGame.LevelId;
                    return sprite;
                }
            }

            return await GetCoverArtSprite(hostedGame.LevelId, token);
        }

        public static async Task<Sprite> GetCoverArtSprite(string levelId, CancellationToken token)
        {
            if (String.IsNullOrEmpty(levelId))
            {
                return null;
            }

            var level = SongCore.Loader.GetLevelById(levelId);

            if (level != null)
            {
                // Official level, or installed custom level found
                var sprite = await level.GetCoverImageAsync(token);
                sprite.name = levelId;
                return sprite;
            }

            // Level not found locally; ask Beat Saver for cover art
            var downloadedCover = await BeatSaverHelper.FetchCoverArtBytesForLevel(levelId, token);

            if (downloadedCover != null)
            {
                var sprite = Sprites.LoadSpriteRaw(downloadedCover);
                sprite.name = levelId;
                return sprite;
            }

            // Failed to get level info, can't set cover art, too bad, very sad
            return null;
        }

        public static async Task<byte[]> FetchCoverArtBytesForUrl(string coverUrl, CancellationToken token)
        {
            try
            {
                var coverResponse = await Plugin.HttpClient.GetAsync(coverUrl, token).ConfigureAwait(false);
                return await coverResponse.Content.ReadAsByteArrayAsync();
            }
            catch (TaskCanceledException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"⚠ Cover art request error: {coverUrl} → {ex}");
                return null;
            }
        }
    }
}
