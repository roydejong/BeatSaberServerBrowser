using UnityEngine;

namespace ServerBrowser.Util
{
    internal static class BssbColors
    {
        public static readonly Color White = Color.white;
        public static readonly Color InactiveGray = MakeUnityColor(255, 255, 255, 0.749f);
        public static readonly Color VeryLightGray = MakeUnityColor(255, 255, 255, .5f);

        internal static Color MakeUnityColor(int r, int g, int b, float alpha = 1f)
            => new Color(r / 255f, g / 255f, b / 255f, alpha);
    }
}