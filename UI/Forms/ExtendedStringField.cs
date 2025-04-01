using System;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Tags.Settings;
using HMUI;
using ServerBrowser.Assets;
using TMPro;
using UnityEngine;

namespace ServerBrowser.UI.Forms
{
    public class ExtendedStringField : ExtendedField<string?>
    {
        private string? _value;
        private readonly StringSetting _stringSetting;
        private readonly TextMeshProUGUI _labelText;

        public override bool Visible
        {
            get => _stringSetting.gameObject.activeSelf;
            set => _stringSetting.gameObject.SetActive(value);
        }

        public string Label
        {
            get => _labelText.text;
            set => _labelText.text = value;
        }

        public override string? Value
        {
            get => _value;
            set
            {
                _value = value;
                _stringSetting.EnterPressed(_value); // this will update both keyboard text & button face text
            }
        }
        
        public override event EventHandler<string?>? OnChange;
        
        public ExtendedStringField(Transform parent, string label, string? initialValue)
        {
            // Base
            var stringTagObj = (new StringSettingTag()).CreateObject(parent);
            ((stringTagObj.transform as RectTransform)!).sizeDelta = new Vector2(90.0f, 7.0f);
            _stringSetting = stringTagObj.GetComponent<StringSetting>();

            // Label
            _labelText = _stringSetting.GetComponentInChildren<TextMeshProUGUI>();
            _labelText.text = label;

            // Value
            _value = initialValue;
            
            _stringSetting.ModalKeyboard.ClearOnOpen = false;
            _stringSetting.ModalKeyboard.Keyboard.KeyboardText.text = _value;
            _stringSetting.TextMesh.text = _value;
            _stringSetting.TextMesh.richText = false; 
            _stringSetting.TextMesh.alignment = TextAlignmentOptions.Center;

            // Event
            _stringSetting.ModalKeyboard.Keyboard.EnterPressed += (async delegate (string newValue)
            {
                _value = newValue;
                await Task.Delay(1); // we need to run OnChange after BSML's own EnterPressed, and this, well, it works
                OnChange?.Invoke(this, newValue);
            });

            // Make the icon look not-wonky
            var valuePicker = _stringSetting.transform.Find("ValuePicker");
            var editButton = valuePicker.transform.Find("EditButton");
            
            var editButtonIcon = editButton.Find("EditIcon").GetComponent<ImageView>();
            editButtonIcon.sprite = Sprites.Pencil;
            editButtonIcon.transform.localScale = new Vector3(-1.0f, -1.0f, 1.0f);
        }
    }
}