using ServerBrowser.Assets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerBrowser.Utils
{
    public static class CoverArtGrabber
    {
        public static async Task<Sprite> GetCoverArtSprite(string levelId, CancellationToken token)
        {
            var level = SongCore.Loader.GetLevelById(levelId);

            if (level != null)
            {
                // Official level, or installed custom level found
                var sprite = await level.GetCoverImageAsync(token);
                sprite.name = levelId;
                return sprite;
            }

            // Level not found locally; ask Beat Saver for cover art
            var downloadedCover = await BeatSaverHelper.FetchCoverArtBytes(levelId, token);

            if (downloadedCover != null)
            {
                var sprite = Sprites.LoadSpriteRaw(downloadedCover);
                sprite.name = levelId;
                return sprite;
            }

            // Failed to get level info, can't set cover art, too bad, very sad
            return null;
        }
    }
}
