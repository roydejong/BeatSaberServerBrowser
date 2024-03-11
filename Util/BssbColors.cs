using UnityEngine;

namespace ServerBrowser.Util
{
    internal static class BssbColors
    {
        public static readonly Color White = Color.white;
        public static readonly Color InactiveGray = MakeUnityColor(255, 255, 255, 0.749f);
        public static readonly Color VeryLightGray = MakeUnityColor(255, 255, 255, .25f);
        public static readonly Color HighlightBlue = MakeUnityColor(0, 192, 255);
        public static readonly Color BssbAccent = MakeUnityColor(207, 3, 137);
        public static readonly Color Orange = MakeUnityColor(255, 95, 0);
        public static readonly Color HoverGradientEnd = MakeUnityColor(255, 255, 255, 0.5f);
        public static readonly Color ButtonBaseBg = MakeUnityColor(0, 0, 0, 0.5f);
        public static readonly Color ButtonBaseBgHover = MakeUnityColor(255, 255, 255, 0.3f);

        internal static Color MakeUnityColor(int r, int g, int b, float alpha = 1f)
            => new Color(r / 255f, g / 255f, b / 255f, alpha);
    }
}