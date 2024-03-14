using System;
using JetBrains.Annotations;
using ServerBrowser.Util;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkLoadingControl : LayoutComponent
    {
        [Inject] private readonly SiraLog _logger = null!;
        // Note: GameServerBrowser is the base game's unused (or dev-only?) server browser, unrelated to the mod
        [Inject] private readonly GameServerBrowserViewController _gameServerBrowserViewController = null!;
        
        private GameObject? _gameObject;
        private LoadingControl? _loadingControl;
        private LayoutElement? _layoutElement;
        
        public event Action? RefreshClickedEvent;
        
        public override void AddToContainer(LayoutContainer container)
        {
            if (_gameServerBrowserViewController == null)
            {
                _logger.Error("Update needed: GameServerBrowserViewController not found!");
                return;
            }

            var loadingControl = _gameServerBrowserViewController._mainLoadingControl;
            _gameObject = Object.Instantiate(loadingControl.gameObject, container.Transform, false);
            _gameObject.name = "TkLoadingControl";
            _gameObject.SetActive(true);
            
            _loadingControl = _gameObject.GetComponent<LoadingControl>();
            _loadingControl.gameObject.SetActive(true);
            _loadingControl.Hide();
            
            _layoutElement = _loadingControl.gameObject.GetOrAddComponent<LayoutElement>();
            
            _loadingControl.didPressRefreshButtonEvent += () => RefreshClickedEvent?.Invoke();
        }

        public void ShowLoading(string text)
        {
            if (_loadingControl != null)
                _loadingControl.ShowLoading(text);
        }

        public void ShowText(string text, bool showRefresh = true)
        {
            if (_loadingControl != null)
                _loadingControl.ShowText(text, showRefresh);
        }
        
        public void SetPreferredHeight(float height)
        {
            if (_layoutElement != null)
                _layoutElement.preferredHeight = height;
        }

        public void Hide()
        {
            if (_loadingControl != null)
                _loadingControl.Hide();
        }
    }
}