using UnityEngine;

namespace ServerBrowser.Util
{
    internal static class BssbColors
    {
        public static readonly Color White = Color.white;
        public static readonly Color WhiteTransparent = MakeUnityColor(255, 255, 255, 0f);
        public static readonly Color VeryLightGray = MakeUnityColor(255, 255, 255, .25f);
        public static readonly Color InactiveGray = MakeUnityColor(255, 255, 255, 0.749f);
        public static readonly Color HighlightBlue = MakeUnityColor(0, 192, 255);
        public static readonly Color BssbAccent = MakeUnityColor(207, 3, 137);
        public static readonly Color Orange = MakeUnityColor(255, 95, 0);
        public static readonly Color HoverGradientEnd = MakeUnityColor(255, 255, 255, 0.5f);
        public static readonly Color ButtonBaseBg = MakeUnityColor(0, 0, 0, 0.5f);
        public static readonly Color ButtonBaseBgHover = MakeUnityColor(255, 255, 255, 0.3f);
        
        public static readonly Color HotPink = MakeUnityColor(223, 21, 111);
        public static readonly Color DarkBlue = MakeUnityColor(12, 120, 242);
        
        // #FF2020FF in rgba is , 255
        
        
        /// <summary>
        /// Hover effect color for search input, filter input in base game.
        /// </summary>
        public static readonly Color SuperPink = MakeUnityColor(253, 37, 225);

        /// <summary>
        /// Failure/error color, used for level fail in base game.
        /// </summary>
        public static readonly Color FailureRed = MakeUnityColor(255, 32, 32);
        
        public static readonly Color GoingToAnotherDimension = MakeUnityColor(133, 44, 183);
        
        // 852CB7FF = 
        

        internal static Color MakeUnityColor(int r, int g, int b, float alpha = 1f)
            => new Color(r / 255f, g / 255f, b / 255f, alpha);
    }
}