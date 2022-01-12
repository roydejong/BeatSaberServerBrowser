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
        public ImageView Image { get; private set; } = null!;

        private CurvedTextMeshPro _titleText = null!;
        private CurvedTextMeshPro _secondaryText = null!;

        public string TitleText
        {
            get => _titleText.text;
            set => _titleText.text = value;
        }

        public string SecondaryText
        {
            get => _secondaryText.text;
            set => _secondaryText.text = value;
        }
        
        private void InitUI()
        {
            // Hover hints
            foreach (var hoverHint in GetComponentsInChildren<HoverHint>())
                Destroy(hoverHint);
            
            // Parts
            Image = transform.Find("SongArtwork").GetComponent<ImageView>();
            
            var singleLineTextContainer = transform.Find("SingleLineTextContainer");
            singleLineTextContainer.gameObject.SetActive(true);
            
            _titleText = singleLineTextContainer.Find("SongNameText").GetComponent<CurvedTextMeshPro>();
            _secondaryText = singleLineTextContainer.Find("AuthorNameText").GetComponent<CurvedTextMeshPro>();
            
            transform.Find("MultipleLineTextContainer").gameObject.SetActive(false);
        }
        #endregion
    }
}