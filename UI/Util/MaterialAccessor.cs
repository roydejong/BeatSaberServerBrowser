using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Util
{
    public class MaterialAccessor
    {
        [Inject] private readonly StandardLevelDetailViewController _levelDetailViewController = null!;
        
        private Material? _uiNoGlowRoundEdge = null;

        public Material? UINoGlowRoundEdge
        {
            get
            {
                if (_uiNoGlowRoundEdge == null && _levelDetailViewController != null)
                {
                    var source = _levelDetailViewController.transform.Find("LevelDetail/LevelBarBig/SongArtwork");
                    var imageView = source != null ? source.GetComponent<HMUI.ImageView>() : null;

                    if (imageView != null)
                        _uiNoGlowRoundEdge = new Material(imageView.material);
                }
                
                if (_uiNoGlowRoundEdge == null)
                    Plugin.Log.Error("NOPE STILL A NO");
                
                return _uiNoGlowRoundEdge;
            }
        }
    }
}