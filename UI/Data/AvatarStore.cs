using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ServerBrowser.UI.Data
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AvatarStore
    {
        private readonly Dictionary<string, Sprite?> _avatarCache = new();

        public async Task<Sprite?> LoadAvatarAsync(string url)
        {
            if (_avatarCache.TryGetValue(url, out var cachedSprite))
                if (cachedSprite != null)
                    return cachedSprite;
            
            var textureDownload = await DownloadTextureFromUrl(url);
            if (textureDownload == null)
                return null;
            
            var sprite = Sprite.Create(textureDownload, new Rect(0, 0, textureDownload.width, textureDownload.height), new Vector2(0.5f, 0.5f));
            _avatarCache[url] = sprite;
            return sprite;
        }

        private static Task<Texture2D?> DownloadTextureFromUrl(string url)
        {
            TaskCompletionSource<Texture2D?> tcs = new();

            var handler = new DownloadHandlerTexture();
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET)
            {
                downloadHandler = handler
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
    }
}