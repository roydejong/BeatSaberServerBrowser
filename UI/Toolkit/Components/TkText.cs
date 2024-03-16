using BeatSaberMarkupLanguage;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkText : LayoutComponent
    {
        private GameObject? _gameObject;
        private CurvedTextMeshPro? _textMesh;
        private LayoutElement? _layoutElement;
        
        public override void AddToContainer(LayoutContainer container)
        {
            _gameObject = new("TkText")
            {
                layer = LayoutContainer.UiLayer
            };
            _gameObject.transform.SetParent(container.Transform, false);
            
            _textMesh = _gameObject.AddComponent<CurvedTextMeshPro>();
            _textMesh.font = BeatSaberUI.MainTextFont;
            _textMesh.fontSharedMaterial = BeatSaberUI.MainUIFontMaterial;
            _textMesh.text = "...";
            _textMesh.fontSize = 4;
            _textMesh.color = Color.white;
            _textMesh.fontStyle = FontStyles.Italic;
            _textMesh.overflowMode = TextOverflowModes.Ellipsis;
            _textMesh.horizontalAlignment = HorizontalAlignmentOptions.Left;
            _textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
            _textMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _textMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            
            _layoutElement = _gameObject.AddComponent<LayoutElement>();
            
        }

        public void SetText(string text)
        {
            if (_textMesh != null)
            {
                _textMesh.SetText(text);
                _textMesh.ForceMeshUpdate();
            }
        }

        public void SetTextColor(Color color)
        {
            if (_textMesh != null)
                _textMesh.color = color;
        }

        public void SetFontSize(float size)
        {
            if (_textMesh != null)
                _textMesh.fontSize = size;
        }

        public void SetFontStyle(FontStyles fontStyle)
        {
            if (_textMesh != null)
                _textMesh.fontStyle = fontStyle;
        }

        public void SetPreferredSize(float? width, float? height)
        {
            if (_layoutElement == null)
                return;

            if (width.HasValue)
            {
                _layoutElement.preferredWidth = width.Value;
            }

            if (height.HasValue)
            {
                _layoutElement.preferredHeight = height.Value;
            }
        }

        public void SetTextAlignment(TextAlignmentOptions textAlignment)
        {
            if (_textMesh != null)
                _textMesh.alignment = textAlignment;
        }
    }
}