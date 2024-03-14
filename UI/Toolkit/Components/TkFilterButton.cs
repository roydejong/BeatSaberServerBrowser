using System;
using BGLib.Polyglot;
using HMUI;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkFilterButton : LayoutComponent
    {
        [Inject] private readonly SiraLog _logger = null!;
        [Inject] private readonly LevelSearchViewController _levelSearchViewController = null!;

        private GameObject? _gameObject;
        private NoTransitionsButton? _button;
        private CurvedTextMeshPro? _placeholderText;
        private CurvedTextMeshPro? _valueText;
        private NoTransitionsButton? _clearButton;

        public event Action? ClickedEvent;
        public event Action? ClearedEvent;
        
        public override void AddToContainer(LayoutContainer container)
        {
            var filterButton = _levelSearchViewController.transform.Find("Filters/FilterButton");
            if (filterButton == null)
            {
                _logger.Error("Update needed: FilterButton base component not found!");
                return;
            }

            _gameObject = Object.Instantiate(filterButton.gameObject, container.Transform, false);
            _gameObject.name = "TkFilterButton";
            _gameObject.SetActive(true);
            
            _button = _gameObject.GetComponent<NoTransitionsButton>();
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                ClickedEvent?.Invoke();
            });

            var tPlaceholderText = _gameObject.transform.Find("PlaceholderText");
            var tText = _gameObject.transform.Find("Text");
            var tClear = _gameObject.transform.Find("ClearButton");

            _placeholderText = tPlaceholderText.GetComponent<CurvedTextMeshPro>();
            Object.Destroy(tPlaceholderText.GetComponent<LocalizedTextMeshProUGUI>());
            
            _valueText = tText.GetComponent<CurvedTextMeshPro>();

            _clearButton = tClear.GetComponent<NoTransitionsButton>();
            _clearButton.onClick.RemoveAllListeners();
            _clearButton.onClick.AddListener(() =>
            {
                SetTextValue("");
                ClearedEvent?.Invoke();
            });
            
            SetTextValue("");
        }
        
        public void SetPlaceholderText(string text)
        {
            if (_placeholderText != null)
                _placeholderText.SetText(text);
        }
        
        public void SetTextValue(string text)
        {
            var hasValue = !string.IsNullOrEmpty(text);
            
            if (_placeholderText != null)
                _placeholderText.gameObject.SetActive(!hasValue);
            
            if (_clearButton != null)
                _clearButton.gameObject.SetActive(hasValue);

            if (_valueText == null)
                return;
            
            _valueText.SetText(text);
            _valueText.gameObject.SetActive(hasValue);
        }
    }
}