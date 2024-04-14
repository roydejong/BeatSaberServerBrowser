using System;
using HMUI;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Forms
{
    public abstract class FormExtender : ITickable, IDisposable
    {
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;
            
        private ViewController _viewController = null!;
        private VerticalLayoutGroup _outerWrapper = null!;
        private VerticalLayoutGroup _innerWrapper = null!;
        private ContentSizeFitter _contentSizeFitter = null!;
        private Transform _spaceBeforeContent = null!;
        private Transform _spaceBeforeButtons = null!;
        private TkDropdownControl _serverSelectorControl = null!;

        protected LayoutContainer Container { get; private set; } = null!;
        
        public int PrependSiblingIndex => _outerWrapper == _innerWrapper ?
            _spaceBeforeContent.GetSiblingIndex() + 1 : 0;
        public int NextSiblingIndex => _spaceBeforeButtons.GetSiblingIndex();
        
        private bool _layoutDirtyOuter;
        private bool _layoutDirtyInner;

        public event Action? MasterServerSwitchRequestedEvent;
        
        protected void Initialize(ViewController viewController)
        {
            _viewController = viewController;
            _viewController.didActivateEvent += HandleViewActivatedEvent;
            
            // Each of the form view controllers has a direct "Wrapper" child that is a VLayout
            _outerWrapper = _viewController.transform.Find("Wrapper").GetComponent<VerticalLayoutGroup>();
            _innerWrapper = _outerWrapper;

            // The inner wrapper is equal to the outer wrapper, except when it's the create server form view
            foreach (Transform transform in _outerWrapper.transform)
            {
                if (!transform.name.Contains("FormView"))
                    continue;
                _innerWrapper = transform.GetComponent<VerticalLayoutGroup>();
                break;
            }
            
            // Inner wrapper should have a content size fitter for dynamic resize
            _contentSizeFitter = _innerWrapper.gameObject.GetOrAddComponent<ContentSizeFitter>();

            // Init container wrapper
            Container = new LayoutContainer(_layoutBuilder, _innerWrapper.transform, false);
            
            // Find first/last space in transform
            foreach (Transform transform in _outerWrapper.transform)
            {
                if (transform.name != "Space")
                    continue;
                
                if (_spaceBeforeContent == null)
                {
                    _spaceBeforeContent = transform; // first space
                    (_spaceBeforeContent.transform as RectTransform)!.sizeDelta = new Vector2(1f, 5f);
                }
                
                _spaceBeforeButtons = transform; // last space
            }
            foreach (Transform transform in _innerWrapper.transform)
            {
                if (transform.name == "Space")
                    _spaceBeforeButtons = transform; // last space
            }

            // Insert server selector control
            var margin = Container.InsertMargin(1f, 5f);
            margin.SetSiblingIndex(PrependSiblingIndex);
            
            _serverSelectorControl = Container.AddDropdownControl("Master Server");
            _serverSelectorControl.GameObject.transform.SetSiblingIndex(PrependSiblingIndex);
            
            // Enqueue full layout update for activation
            MarkLayoutDirty();
        }
        
        public void Dispose()
        {
            _viewController.didActivateEvent -= HandleViewActivatedEvent;
        }

        private void HandleViewActivatedEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _serverSelectorControl.RemoveAllOnClickActions();
            _serverSelectorControl.AddOnClick(() => MasterServerSwitchRequestedEvent?.Invoke());
        }

        protected void MarkLayoutDirty()
        {
            if (_outerWrapper != null && !_outerWrapper.enabled)
                _outerWrapper.enabled = true;

            if (_innerWrapper != null && !_innerWrapper.enabled)
                _innerWrapper.enabled = true;

            if (_contentSizeFitter != null && !_contentSizeFitter.enabled)
                _contentSizeFitter.enabled = true;

            _layoutDirtyOuter = true;
            _layoutDirtyInner = true;
        }

        public void Tick()
        {
            if (!_viewController.isActivated)
                return;
            
            if (_layoutDirtyOuter && _outerWrapper != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_outerWrapper.transform as RectTransform);
                _layoutDirtyOuter = false;
                return;
            }

            if (_layoutDirtyInner && _innerWrapper != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_innerWrapper.transform as RectTransform);
                _layoutDirtyInner = false;
                return;
            }
        }
    }
}