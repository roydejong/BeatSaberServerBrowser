using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.Assets
{
    internal static class Sprites
    {
        private const string ResourcePrefix = "ServerBrowser.Assets.Sprites.";

        public const string Avatar = "Avatar"; 
        public const string ComboCrown = "ComboCrown";
        public const string Crown = "Crown";
        public const string Friends = "Friends";
        public const string Ghost = "Ghost";
        public const string Global = "Global";
        public const string Lock = "Lock";
        public const string Player = "Player";
        public const string Plus = "Plus";
        public const string Random = "Random";
        public const string SaberClash = "SaberClash";
        public const string SaberUp = "SaberUp";
        public const string Search = "Search";
        public const string Spectator = "Spectator";
        public const string SuperFast = "SuperFast";

        private static Dictionary<string, Sprite> _loadedSprites = new();

        public static async Task PreloadAsync()
        {
            // TODO LoadAsync any sprites we want to be in cache ahead of time
        }

        public static async Task<Sprite?> LoadAsync(string spriteName)
        {
            var spritePath = $"{ResourcePrefix}{spriteName}.png";
            
            if (_loadedSprites.TryGetValue(spritePath, out var cachedSprite))
                if (cachedSprite != null)
                    return cachedSprite;
            
            var sprite = await Utilities.LoadSpriteFromAssemblyAsync(spritePath);
            if (sprite != null)
                _loadedSprites[spritePath] = sprite;
            return sprite;
        }

        public static async Task SetSpriteAsync(this Image image, string spriteName)
        {
            var sprite = await LoadAsync(spriteName);

            if (sprite == null || image == null)
                return;
            
            image.sprite = sprite;
        }
    }
}