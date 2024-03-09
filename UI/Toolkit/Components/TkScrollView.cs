using HMUI;
using ServerBrowser.UI.Toolkit.Scripts;
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
        [Inject] private readonly SiraLog _logger = null!;
        [Inject] private readonly DiContainer _diContainer = null!;
        [Inject] private readonly PrivacyPolicyDisplayViewController _policyViewController = null!;

        private GameObject? _gameObject;
        private ScrollView? _scrollView;
        
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
            Plugin.Log.Error("ooop 1");
            
            var viewport = _gameObject.transform.Find("Viewport");
            Plugin.Log.Error("ooop 2");
            
            // Remove text content from clone
            viewport.Find("Text").gameObject.SetActive(false);
            
            // Create and wrap our own content container
            var viewportWrap = new LayoutContainer(container.Builder, viewport, false);
            Content = viewportWrap.AddVerticalLayoutGroup("Content", expandChildWidth: true,
                childAlignment: TextAnchor.UpperLeft, horizontalFit: ContentSizeFitter.FitMode.Unconstrained,
                verticalFit: ContentSizeFitter.FitMode.PreferredSize, pivotPoint: new Vector2(0, 1f));
            
            // Auto resize on content rect change
            var resizeListener = Content.GameObject.AddComponent<ScrollViewRectChangeListener>();
            resizeListener.BindScrollView(_scrollView);

            // Make interactable
            _diContainer.InstantiateComponent<VRGraphicRaycaster>(viewport.gameObject);
            
            // Rebind content rect
            _scrollView._contentRectTransform = Content.RectTransform;
            _scrollView._scrollType = ScrollView.ScrollType.PageSize;
            
            // Enable
            _gameObject.SetActive(true);
        }

        public override void SetActive(bool active)
        {
            if (_gameObject != null)
                _gameObject.SetActive(active);
        }
    }
}