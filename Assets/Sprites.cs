using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ServerBrowser.Assets
{
    /// <summary>
    /// Helper code taken from BeatSaverDownloader
    /// Copyright (c) 2018 andruzzzhka (MIT Licensed)
    /// </summary>
    internal static class Sprites
    {
        public static Sprite? BeatSaverIcon;
        
        /// Technology - Straight Line icon set by designforeat
        /// https://www.iconfinder.com/iconsets/technology-straight-line
        /// License: CC BY 3.0
        public static Sprite? Portal;
        public static Sprite? PortalUser;
        
        /// Octicons icon set by Github
        /// Copyright (c) 2020 GitHub Inc.
        public static Sprite? Pencil;

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            IsInitialized = true;

            //BeatSaverIcon = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.BeatSaver.png");
            //Portal = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Portal.png");
            //PortalUser = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.PortalUser.png");
            Pencil = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Pencil.png");
        }

        private static Sprite? LoadSpriteFromResources(string resourcePath, float pixelsPerUnit = 100.0f)
        {
            var rawData = GetResource(Assembly.GetCallingAssembly(), resourcePath);

            if (rawData is null)
                return null;
            
            var sprite = LoadSpriteRaw(rawData, pixelsPerUnit);

            if (sprite is null)
                return null;
            
            sprite.name = resourcePath;
            return sprite;
        }

        private static byte[]? GetResource(Assembly asm, string resourceName)
        {
            var stream = asm.GetManifestResourceStream(resourceName);

            if (stream is null)
                return null;
            
            var data = new byte[stream.Length];
            stream.Read(data, 0, (int) stream.Length);
            return data;
        }

        private static Sprite? LoadSpriteRaw(byte[] image, float pixelsPerUnit = 100.0f)
        {
            var texture = LoadTextureRaw(image);

            if (texture is null)
                return null;
            
            return LoadSpriteFromTexture(texture, pixelsPerUnit);
        }

        private static Texture2D? LoadTextureRaw(byte[] file)
        {
            if (!file.Any())
                return null;
            
            var texture = new Texture2D(2, 2);
            return texture.LoadImage(file) ? texture : null;
        }

        private static Sprite LoadSpriteFromTexture(Texture2D spriteTexture, float pixelsPerUnit = 100.0f)
        {
            return Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height),
                new Vector2(0, 0), pixelsPerUnit);
        }
    }
}