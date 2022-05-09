using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Utils
{
    public static class BeatSaberUIExtensions
    {
        public static void SetButtonFaceColor(this Button button, Color color)
        {
            var textMesh = button.GetComponentInChildren<TextMeshProUGUI>();

            if (textMesh == null)
                return;

            textMesh.faceColor = color;
        }
    }
}