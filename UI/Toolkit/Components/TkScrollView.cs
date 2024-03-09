using HMUI;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Zenject;
using Object = UnityEngine.Object;

namespace ServerBrowser.UI.Toolkit.Components
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TkScrollView : LayoutComponent
    {
        [Inject] protected readonly SiraLog _logger = null!;
        [Inject] protected readonly DiContainer _diContainer = null!;
        [Inject] protected readonly PrivacyPolicyDisplayViewController _policyViewController = null!;

        protected GameObject? _gameObject;
        protected ScrollView? _scrollView;
        
        public LayoutContainer? Content { get; private set; }
        
        public override void AddToContainer(LayoutContainer container)
        {
            if (_policyViewController == null)
            {
                _logger.Error("Update needed: PrivacyPolicyDisplayViewController base component not found!");
                return;
            }
            
            var baseScrollView = _policyViewController.transform.Find("TextPageScrollView");
            _gameObject = Object.Instantiate(baseScrollView.gameObject, container.Transform);
            _gameObject.name = "TkScrollView";
            _gameObject.SetActive(false);
            
            _scrollView = _gameObject.GetComponent<ScrollView>();
            _diContainer.Inject(_scrollView);
            
            var viewport = _gameObject.transform.Find("Viewport");
            
            // Remove text content from clone
            viewport.Find("Text").gameObject.SetActive(false);
            
            // Create and wrap our own content container
            var contentGameObject = new GameObject("Content")
            {
                layer = LayoutContainer.UiLayer
            };
            contentGameObject.transform.SetParent(viewport, false);
            contentGameObject.AddComponent<LayoutElement>();
            Content = new LayoutContainer(container.Builder, contentGameObject.transform, false);

            // Make interactable
            _diContainer.InstantiateComponent<VRGraphicRaycaster>(viewport.gameObject);
            
            // Rebind content rect
            _scrollView._contentRectTransform = Content.RectTransform;
            
            // Set content rect pivot
            var rectTransform = (Content.GameObject.transform as RectTransform)!;
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.sizeDelta = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 1f);
            
            // Enable
            _gameObject.SetActive(true);
        }

        public override void SetActive(bool active)
        {
            if (_gameObject != null)
                _gameObject.SetActive(active);
        }

        public void SetContentHeight(float height)
        {
            if (_scrollView == null)
                return;
            
            _scrollView.SetContentSize(height);
            _scrollView.ScrollTo(0.0f, false);
            _scrollView.RefreshButtons();
        }

        public void RefreshContentHeight()
        {
            if (_scrollView == null)
                return;
            
            _scrollView.UpdateContentSize();
            _scrollView.RefreshButtons();
        }
    }
}