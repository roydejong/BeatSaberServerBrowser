using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ServerBrowser.Assets
{
    /// <summary>
    /// Utilities for using embedded and downloaded sprite assets in the UI.
    /// </summary>
    /// <remarks>
    /// Helper code taken from BeatSaverDownloader
    /// Copyright (c) 2018 andruzzzhka (MIT Licensed)
    /// </remarks>
    internal static class Sprites
    {
        /// Announce icon
        /// Icon by RemixIcon (https://www.iconfinder.com/iconsets/remixicon-media)
        /// Licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/)
        public static Sprite? Announce;

        /// BSSB logo
        public static Sprite? ServerBrowserLogo;

        /// BeatSaver logo
        public static Sprite? BeatSaverLogo;

        /// Crown icon
        /// Icon by Ivan Boyko (https://www.iconfinder.com/visualpharm)
        /// Licensed under CC BY 3.0 (https://creativecommons.org/licenses/by/3.0/)
        public static Sprite? Crown;

        /// Pencil icon
        /// Icon by Github, MIT Licensed
        /// Copyright (c) 2020 GitHub Inc
        public static Sprite? Pencil;

        /// Person icon
        /// Icon by Ivan Boyko (https://www.iconfinder.com/visualpharm)
        /// Licensed under CC BY 3.0 (https://creativecommons.org/licenses/by/3.0/)
        public static Sprite? Person;

        /// Portal icon
        /// Straight Line icon set by designforeat (https://www.iconfinder.com/iconsets/technology-straight-line)
        /// Licensed under CC BY 3.0 (https://creativecommons.org/licenses/by/3.0/)
        public static Sprite? Portal;

        /// Portal user icon
        /// Straight Line icon set by designforeat (https://www.iconfinder.com/iconsets/technology-straight-line)
        /// Licensed under CC BY 3.0 (https://creativecommons.org/licenses/by/3.0/)
        public static Sprite? PortalUser;

        /// Robot icon
        /// Fluent Solid 24px vol.1 icon pack by Microsoft (https://www.iconfinder.com/iconsets/fluent-solid-24px-vol-1)
        /// Licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/)
        public static Sprite? Robot;

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            IsInitialized = true;

            Announce = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Announce.png");
            ServerBrowserLogo = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.BSSB.png");
            BeatSaverLogo = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.BeatSaver.png");
            Crown = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Crown.png");
            Pencil = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Pencil.png");
            Person = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Person.png");
            Portal = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Portal.png");
            PortalUser = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.PortalUser.png");
            Robot = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Robot.png");
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

        internal static Sprite? LoadSpriteRaw(byte[] image, float pixelsPerUnit = 100.0f,
            SpriteMeshType spriteMeshType = SpriteMeshType.Tight)
        {
            var texture = LoadTextureRaw(image);

            if (texture is null)
                return null;

            return LoadSpriteFromTexture(texture, pixelsPerUnit, spriteMeshType);
        }

        private static Texture2D? LoadTextureRaw(byte[] file)
        {
            if (!file.Any())
                return null;

            var texture = new Texture2D(2, 2);
            return texture.LoadImage(file) ? texture : null;
        }

        private static Sprite LoadSpriteFromTexture(Texture2D spriteTexture, float pixelsPerUnit = 100.0f,
            SpriteMeshType spriteMeshType = SpriteMeshType.Tight)
        {
            return Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height),
                new Vector2(0, 0), pixelsPerUnit, 0, spriteMeshType);
        }
    }
}