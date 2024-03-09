using HMUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ServerBrowser.UI.Toolkit.Scripts
{
    public class ScrollViewRectChangeListener : UIBehaviour
    {
        private ScrollView? _scrollView = null;
        private bool _isDirty = false;
        private int _lastHeight = 0;
        private int _lastHeightPost = 0;
        
        protected override void OnRectTransformDimensionsChange()
        {
            _isDirty = true;
        }

        private void Update()
        {
            if (!_isDirty || _scrollView == null)
                return;
            
            var curHeight = CurrentHeight;
            var shouldUpdate = curHeight != _lastHeight;

            if (shouldUpdate)
            {
                _scrollView.UpdateContentSize();
                _scrollView.RefreshButtons();
            }
            
            // NOTE: Size might change, and OnRectTransformDimensionsChange() will fire again after we update scrollView
            // I don't know why, but that's why we record and compare against the last height from before our update

            _lastHeight = curHeight;
            _isDirty = false;
        }

        public int CurrentHeight => (int)(transform as RectTransform)!.rect.height;

        public void BindScrollView(ScrollView scrollView)
        {
            _scrollView = scrollView;
        }
    }
}