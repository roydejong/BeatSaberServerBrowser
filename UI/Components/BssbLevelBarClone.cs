using System.Linq;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Components
{
    public class BssbLevelBarClone : MonoBehaviour
    {
        #region Template/init
        private static GameObject? _templateCached;

        private static GameObject Template
        {
            get
            {
                if (_templateCached == null)
                {
                    _templateCached = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>()
                        .First(x => x.gameObject.name == "LevelDetail")
                        .transform.Find("LevelBarBig")
                        .gameObject;
                }

                return _templateCached;
            }
        }

        public static BssbLevelBarClone Create(DiContainer container, Transform parent, bool occupyLayoutSpace = false)
        {
            var clone = Instantiate(Template, parent);
            clone.gameObject.name = "BssbLevelBarClone";

            if (occupyLayoutSpace)
            {
                var layoutElement = clone.gameObject.AddComponent<LayoutElement>();
                layoutElement.minWidth = 120;
                layoutElement.minHeight = 14;
            }

            clone.gameObject.SetActive(true);
            
            var script = clone.gameObject.AddComponent<BssbLevelBarClone>();
            script.InitUI();
            return script;
        }
        #endregion

        #region UI Access
        private ImageView _bg = null!;
        private ImageView _image = null!;
        private Transform _textContainer = null!;
        private CurvedTextMeshPro _titleText = null!;
        private CurvedTextMeshPro _secondaryText = null!;
        
        private void InitUI()
        {
            // Hover hints
            foreach (var hoverHint in GetComponentsInChildren<HoverHint>())
                Destroy(hoverHint);
            
            // Parts
            _bg = transform.Find("BG").GetComponent<ImageView>();
            _image = transform.Find("SongArtwork").GetComponent<ImageView>();
            
            transform.Find("MultipleLineTextContainer").gameObject.SetActive(false);
            _textContainer = transform.Find("SingleLineTextContainer");
            _textContainer.gameObject.SetActive(true);
            
            _titleText = _textContainer.Find("SongNameText").GetComponent<CurvedTextMeshPro>();
            _secondaryText = _textContainer.Find("AuthorNameText").GetComponent<CurvedTextMeshPro>();
            
            // Enable rich text for detail text
            _secondaryText.richText = true;
        }

        public enum BackgroundStyle : byte
        {
            GameDefault,
            ColorfulGradient,
            GrayTitle,
            SolidBlue,
            SolidCerise
        }
        
        public void SetBackgroundStyle(BackgroundStyle style = BackgroundStyle.GameDefault, bool skew = true,
            bool enableRaycast = false, bool padLeft = false)
        {
            // Primary background color
            _bg.color = style switch
            {
                BackgroundStyle.ColorfulGradient => Color.white,
                BackgroundStyle.GrayTitle => new Color(1, 1, 1, .2f),
                BackgroundStyle.SolidBlue => new Color(52f / 255f, 31f / 255f, 151f / 255f),
                BackgroundStyle.SolidCerise => new Color(207f / 255f, 3f / 255f, 137f / 255f),
                _ => Color.black
            };
            // Gradient left color
            _bg.color0 = style switch
            {
                BackgroundStyle.ColorfulGradient => new Color(0, .55f, .99f, 0f),
                BackgroundStyle.GrayTitle => Color.white,
                BackgroundStyle.SolidBlue => Color.white,
                BackgroundStyle.SolidCerise => Color.white,
                _ => new Color(1, 1, 1, 0)
            };
            // Gradient right color
            _bg.color1 = style switch
            {
                BackgroundStyle.ColorfulGradient => new Color(1f, 0, .5f, 1f),
                BackgroundStyle.GrayTitle => new Color(1, 1, 1, 0),
                BackgroundStyle.SolidBlue => Color.white,
                BackgroundStyle.SolidCerise => Color.white,
                _ => new Color(1, 1, 1, .3f)
            };
            // Skew
            _bg.SetField("_skew", (skew ? .18f : 0));
            // Pad left
            const float imageBaseX = -59.33f;
            (_image.transform as RectTransform)!.localPosition = new Vector3(padLeft ? (imageBaseX + 13.5f) : imageBaseX, -14, 0);
            const float textBaseX = 3.5f;
            (_textContainer as RectTransform)!.localPosition = new Vector3(padLeft ? (textBaseX + 2f) : 3.5f, -7, 0);
            // Raycast
            _image.raycastTarget = enableRaycast;
            _bg.raycastTarget = enableRaycast;
        }

        public void SetImageVisible(bool visible)
        {
            _image.gameObject.SetActive(visible);
            (_textContainer.transform as RectTransform)!.sizeDelta = new Vector2((visible ? -27f : 0), -2);
            (_bg.transform as RectTransform)!.sizeDelta = (visible ? new Vector2(-4, 0) : Vector2.zero);
        }
        
        public void SetImageSprite(Sprite? sprite)
        {
            _image.sprite = sprite;
        }
        
        public void SetText(string? titleText, string? secondaryText)
        {
            _titleText.SetText(titleText ?? "");
            _secondaryText.SetText(secondaryText ?? "");
        }
        #endregion
    }
}