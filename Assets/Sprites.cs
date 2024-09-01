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

        internal const string Avatar = "Avatar"; 
        internal const string Checkmark = "Checkmark";
        internal const string CircleDot = "CircleDot";
        internal const string ComboCrown = "ComboCrown";
        internal const string Crown = "Crown";
        internal const string EnergyBolt = "EnergyBolt";
        internal const string Friends = "Friends";
        internal const string Ghost = "Ghost";
        internal const string Global = "Global";
        internal const string Lock = "Lock";
        internal const string OfficialServerIcon = "OfficialServerIcon";
        internal const string PlaceholderAvatar = "PlaceholderAvatar";
        internal const string PlaceholderSabers = "PlaceholderSabers";
        internal const string Player = "Player";
        internal const string Plus = "Plus";
        internal const string Random = "Random";
        internal const string SaberClash = "SaberClash";
        internal const string SaberUp = "SaberUp";
        internal const string Search = "Search";
        internal const string Spectator = "Spectator";
        internal const string SuperFast = "SuperFast";

        private static readonly Dictionary<string, Sprite> _loadedSprites = new();

        internal static async Task<Sprite?> LoadAsync(string spriteName)
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

        internal static async Task SetAssetSpriteAsync(this Image image, string spriteName)
        {
            var sprite = await LoadAsync(spriteName);

            if (sprite == null || image == null)
                return;
            
            image.sprite = sprite;
        }
    }
}