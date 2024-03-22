using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ServerBrowser.Assets;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;
using Object = UnityEngine.Object;

namespace ServerBrowser.UI.Toolkit
{
    [UsedImplicitly]
    public class RemoteImageStore : IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;

        private readonly Dictionary<string, Sprite?> _cache = new();

        public async Task<Sprite> LoadImageAsync(string url, string fallbackSprite = Sprites.PlaceholderSabers)
        {
            if (_cache.TryGetValue(url, out var cachedSprite))
                if (cachedSprite != null)
                    return cachedSprite;

            Sprite sprite;
            Texture2D? textureDownload = null;

            try
            {
                textureDownload = await DownloadTextureFromUrl(url);
            }
            catch (Exception)
            {
                // Download failed
            }

            if (textureDownload != null)
            {
                sprite = Sprite.Create(textureDownload, new Rect(0, 0, textureDownload.width, textureDownload.height),
                    new Vector2(0.5f, 0.5f));
            }
            else
            {
                _log.Warn($"Remote image download failed: {url}");
                sprite = (await Sprites.LoadAsync(fallbackSprite))!;
            }

            _cache[url] = sprite;
            return sprite;
        }

        private static Task<Texture2D?> DownloadTextureFromUrl(string url)
        {
            TaskCompletionSource<Texture2D?> tcs = new();

            var handler = new DownloadHandlerTexture();
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET)
            {
                downloadHandler = handler,
                timeout = 5
            };
            request.SendWebRequest().completed += (_) =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    tcs.SetException(new Exception("UnityWebRequest failed"));
                }
                else
                {
                    tcs.SetResult(handler.texture);
                }
            };
            return tcs.Task;
        }

        public void ClearCache()
        {
            foreach (var sprite in _cache.Values)
            {
                if (sprite != null)
                    Object.Destroy(sprite);
            }

            _cache.Clear();
        }

        public void Dispose()
        {
            ClearCache();
        }
    }
}