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
    public class TkSearchInputField : LayoutComponent
    {
        [Inject] private readonly SiraLog _logger = null!;
        [Inject] private readonly LevelSearchViewController _levelSearchViewController = null!;

        private GameObject? _gameObject;
        private InputFieldView? _inputFieldView;
        private CurvedTextMeshPro? _placeholderText;

        public event Action<InputFieldView.SelectionState, string> ChangedEvent;

        public override void AddToContainer(LayoutContainer container)
        {
            var searchInputField = _levelSearchViewController.transform.Find("Filters/SearchInputField");
            if (searchInputField == null)
            {
                _logger.Error("Update needed: SearchInputField base component not found!");
                return;
            }

            _gameObject = Object.Instantiate(searchInputField.gameObject, container.Transform, false);
            _gameObject.name = "TkSearchInputField";
            _gameObject.SetActive(true);

            _inputFieldView = _gameObject.GetComponent<InputFieldView>();
            
            _placeholderText = _inputFieldView._placeholderText.GetComponent<CurvedTextMeshPro>();
            Object.Destroy(_inputFieldView._placeholderText.GetComponent<LocalizedTextMeshProUGUI>());

            _inputFieldView.onValueChanged.RemoveAllListeners();
            _inputFieldView.onValueChanged.AddListener((_) =>
            {
                ChangedEvent?.Invoke(SelectionState, TextValue);
            });
            _inputFieldView.selectionStateDidChangeEvent += state =>
            {
                ChangedEvent?.Invoke(state, TextValue);
            };

            ClearTextValue();
        }

        public InputFieldView.SelectionState SelectionState => _inputFieldView != null
            ? _inputFieldView.selectionState
            : InputFieldView.SelectionState.Normal;

        public string TextValue => _inputFieldView != null ? _inputFieldView.text : "";

        public void SetTextValue(string textValue)
        {
            if (_inputFieldView != null)
                _inputFieldView.SetText(textValue);
        }

        public void ClearTextValue() => SetTextValue("");

        public void SetPlaceholderText(string placeholderText)
        {
            if (_placeholderText != null)
                _placeholderText.SetText(placeholderText);
        }
    }
}