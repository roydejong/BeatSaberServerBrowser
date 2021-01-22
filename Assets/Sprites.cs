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
        public static Sprite BeatSaverIcon;

        public static Sprite Portal;
        public static Sprite PortalUser;

        public static Sprite Pencil;

        public static void Initialize()
        {
            BeatSaverIcon = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.BeatSaver.png");

            /// Technology - Straight Line icon set by designforeat
            /// https://www.iconfinder.com/iconsets/technology-straight-line
            /// License: CC BY 3.0
            
            Portal = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Portal.png");
            PortalUser = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.PortalUser.png");

            /// Octicons icon set by Github
            /// Copyright (c) 2020 GitHub Inc.

            Pencil = LoadSpriteFromResources("ServerBrowser.Assets.Sprites.Pencil.png");
        }

        public static Texture2D LoadTextureRaw(byte[] file)
        {
            if (file.Count() > 0)
            {
                Texture2D Tex2D = new Texture2D(2, 2);
                if (Tex2D.LoadImage(file))
                    return Tex2D;
            }
            return null;
        }

        public static Sprite LoadSpriteFromTexture(Texture2D SpriteTexture, float PixelsPerUnit = 100.0f)
        {
            if (SpriteTexture)
            {
                return Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
            }

            return null;
        }

        public static Sprite LoadSpriteRaw(byte[] image, float PixelsPerUnit = 100.0f)
        {
            return LoadSpriteFromTexture(LoadTextureRaw(image), PixelsPerUnit);
        }

        public static Sprite LoadSpriteFromResources(string resourcePath, float PixelsPerUnit = 100.0f)
        {
            var sprite = LoadSpriteRaw(GetResource(Assembly.GetCallingAssembly(), resourcePath), PixelsPerUnit);
            sprite.name = resourcePath;
            return sprite;
        }

        public static byte[] GetResource(Assembly asm, string ResourceName)
        {
            System.IO.Stream stream = asm.GetManifestResourceStream(ResourceName);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }
    }
}
