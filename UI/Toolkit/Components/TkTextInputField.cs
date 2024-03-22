using System;
using BGLib.Polyglot;
using HMUI;
using JetBrains.Annotations;
using ServerBrowser.Assets;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkTextInputField : LayoutComponent
    {
        [Inject] private readonly SiraLog _logger = null!;
        [Inject] private readonly LevelSearchViewController _levelSearchViewController = null!;

        private GameObject _gameObject = null!;
        private InputFieldView _inputFieldView = null!;
        private CurvedTextMeshPro _placeholderText = null!;
        private ImageView _iconView = null!;
        
        public event Action<InputFieldView.SelectionState, string>? ChangedEvent;
        public event Action<string>? KeyboardOkPressedEvent;

        public override void AddToContainer(LayoutContainer container)
        {
            var searchInputField = _levelSearchViewController.transform.Find("Filters/SearchInputField");
            if (searchInputField == null)
            {
                _logger.Error("Update needed: SearchInputField base component not found!");
                return;
            }

            _gameObject = Object.Instantiate(searchInputField.gameObject, container.Transform, false);
            _gameObject.name = "TkTextInputField";
            _gameObject.SetActive(true);

            _inputFieldView = _gameObject.GetComponent<InputFieldView>();
            
            _placeholderText = _inputFieldView._placeholderText.GetComponent<CurvedTextMeshPro>();
            Object.Destroy(_inputFieldView._placeholderText.GetComponent<LocalizedTextMeshProUGUI>());

            _iconView = _gameObject.transform.Find("Icon").GetComponent<ImageView>();

            _inputFieldView.onValueChanged.RemoveAllListeners();
            _inputFieldView.onValueChanged.AddListener((_) =>
            {
                ChangedEvent?.Invoke(SelectionState, TextValue);
            });
            _inputFieldView.selectionStateDidChangeEvent += state =>
            {
                ChangedEvent?.Invoke(state, TextValue);
            };

            var keyboardManager = GetUIKeyboardManager();
            keyboardManager.keyboard.okButtonWasPressedEvent += () => KeyboardOkPressedEvent?.Invoke(TextValue);

            ClearTextValue();
        }

        public InputFieldView.SelectionState SelectionState => _inputFieldView != null
            ? _inputFieldView.selectionState
            : InputFieldView.SelectionState.Normal;
        public string TextValue => _inputFieldView != null ? _inputFieldView.text : "";
        
        public void SetTextValue(string textValue) => _inputFieldView.SetText(textValue);
        public void ClearTextValue() => SetTextValue("");
        public void SetPlaceholderText(string placeholderText) => _placeholderText.SetText(placeholderText);
        public void SetIconSprite(string spriteName) => _ = _iconView.SetAssetSpriteAsync(spriteName);
        public void OpenKeyboard() => GetUIKeyboardManager().OpenKeyboardFor(_inputFieldView);
        public void SetKeyboardOffset(Vector3 offset) => _inputFieldView._keyboardPositionOffset = offset;
        
        
        private static UIKeyboardManager? UIKeyboardManager = null;
        private static UIKeyboardManager GetUIKeyboardManager()
        {
            if (UIKeyboardManager == null)
                UIKeyboardManager = Object.FindObjectOfType<UIKeyboardManager>();
            return UIKeyboardManager;
        }
    }
}