using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Utils
{
    internal static class BeatSaberUIExtensions
    {
        internal static void SetButtonFaceColor(this Button button, Color color)
        {
            var textMesh = button.transform.Find("Content/Text")
                .GetComponent<TextMeshProUGUI>();

            if (textMesh == null)
                return;

            textMesh.faceColor = color;
        }
        
        internal static void SetButtonUnderlineColor(this Button button, Color? color)
        {
            if (!color.HasValue || color.Equals(Color.white))
            {
                color = new Color(1, 1, 1, .502f); // game default for underline
            }

            var textMesh = button.transform.Find("Underline")
                .GetComponent<ImageView>();

            if (textMesh == null)
                return;

            textMesh.color = color.Value;
        }

        internal static void SetButtonFaceAndUnderlineColor(this Button button, Color color)
        {
            SetButtonFaceColor(button, color);
            SetButtonUnderlineColor(button, color);
        }
    }
}