using System.Linq;
using HMUI;
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
        private CurvedTextMeshPro _titleText = null!;
        private CurvedTextMeshPro _secondaryText = null!;
        
        private void InitUI()
        {
            // Hover hints
            foreach (var hoverHint in GetComponentsInChildren<HoverHint>())
                Destroy(hoverHint);
            
            // Parts
            var singleLineTextContainer = transform.Find("SingleLineTextContainer");
            singleLineTextContainer.gameObject.SetActive(true);
            transform.Find("MultipleLineTextContainer").gameObject.SetActive(false);
            
            _bg = transform.Find("BG").GetComponent<ImageView>();
            _image = transform.Find("SongArtwork").GetComponent<ImageView>();
            _titleText = singleLineTextContainer.Find("SongNameText").GetComponent<CurvedTextMeshPro>();
            _secondaryText = singleLineTextContainer.Find("AuthorNameText").GetComponent<CurvedTextMeshPro>();
        }

        public enum BackgroundStyle : byte
        {
            GameDefault,
            Test,
            GrayTitle
        }
        
        public void SetBackgroundStyle(BackgroundStyle style = BackgroundStyle.GameDefault)
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