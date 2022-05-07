using UnityEngine;

namespace ServerBrowser.UI.Utils
{
    public static class BssbColorScheme
    {
        public static readonly Color Green = MakeUnityColor(46, 204, 113);
        public static readonly Color Gold = MakeUnityColor(241, 196, 15);
        public static readonly Color Red = MakeUnityColor(231, 76, 60);
        public static readonly Color Blue = MakeUnityColor(52, 152, 219);
        public static readonly Color Pinkish = MakeUnityColor(155, 89, 182);
        public static readonly Color MutedGray = MakeUnityColor(188, 189, 194);
        public static readonly Color White = MakeUnityColor(255, 255, 255);

        private static Color MakeUnityColor(int r, int g, int b, float alpha = 1f)
            => new Color(r / 255f, g / 255f, b / 255f, alpha);
    }
}