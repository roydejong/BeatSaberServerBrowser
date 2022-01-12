using System.Linq;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class LevelBarClone : MonoBehaviour
    {
        #region Template/init
        private static GameObject? _templateCached;

        private static GameObject Template
        {
            get
            {
                if (_templateCached is null)
                {
                    _templateCached = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>()
                        .First(x => x.gameObject.name == "LevelDetail")
                        .transform.Find("LevelBarBig")
                        .gameObject;
                }

                return _templateCached;
            }
        }

        public static LevelBarClone Create(Transform parent)
        {
            var levelDetailView = Instantiate(Template, parent);
            levelDetailView.gameObject.name = "BSSBLevelBarClone";
            levelDetailView.gameObject.SetActive(true);
            
            var clone = levelDetailView.gameObject.AddComponent<LevelBarClone>();
            clone.InitUI();
            return clone;
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
        }

        public enum BackgroundStyle : byte
        {
            GameDefault,
            Test,
            GrayTitle
        }
        
        public void SetBackgroundStyle(BackgroundStyle style = BackgroundStyle.GameDefault, bool skew = true)
        {
            // Primary background color
            _bg.color = style switch
            {
                BackgroundStyle.Test => Color.white,
                BackgroundStyle.GrayTitle => new Color(1, 1, 1, .2f),
                _ => Color.black
            };
            // Gradient left color
            _bg.color0 = style switch
            {
                BackgroundStyle.Test => new Color(0, .55f, .99f, 0f),
                BackgroundStyle.GrayTitle => Color.white,
                _ => new Color(1, 1, 1, 0)
            };
            // Gradient right color
            _bg.color1 = style switch
            {
                BackgroundStyle.Test => new Color(1f, 0, .5f, 1f),
                BackgroundStyle.GrayTitle => new Color(1, 1, 1, 0),
                _ => new Color(1, 1, 1, .3f)
            };
            // Skew
            _bg.SetField("_skew", (skew ? .18f : 0));
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
        
        public void SetText(string titleText, string secondaryText)
        {
            _titleText.SetText(titleText);
            _secondaryText.SetText(secondaryText);
        }
        #endregion
    }
}