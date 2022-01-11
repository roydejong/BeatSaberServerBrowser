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

        public override float Height => 15f;

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
            
            _stringSetting.modalKeyboard.clearOnOpen = false;
            _stringSetting.modalKeyboard.keyboard.KeyboardText.text = _value;
            _stringSetting.text.text = _value;
            _stringSetting.text.richText = false;
            _stringSetting.text.alignment = TextAlignmentOptions.Center;

            // Event
            _stringSetting.modalKeyboard.keyboard.EnterPressed += (async delegate (string newValue)
            {
                _value = newValue;
                await Task.Delay(1); // we need to run OnChange after BSML's own EnterPressed, and this, well, it works
                OnChange?.Invoke(this, newValue);
            });

            // Make it look nice :-)
            var valuePicker = _stringSetting.transform.Find("ValuePicker");

            var buttonLeftSide = valuePicker.Find("DecButton") as RectTransform;
            var buttonRightSide = valuePicker.Find("IncButton") as RectTransform;
            var valueText = valuePicker.Find("ValueText") as RectTransform;

            var leftSideWidth = 0.05f;

            buttonLeftSide!.anchorMin = new Vector2(0.0f, 0.0f);
            buttonLeftSide.anchorMax = new Vector2(leftSideWidth, 1.0f);
            buttonLeftSide.offsetMin = new Vector2(0.0f, 0.0f);
            buttonLeftSide.offsetMax = new Vector2(0.0f, 0.0f);
            buttonLeftSide.sizeDelta = new Vector2(0.0f, 0.0f);

            buttonRightSide!.anchorMin = new Vector2(leftSideWidth, 0.0f);
            buttonRightSide.anchorMax = new Vector2(1.0f, 1.0f);
            buttonRightSide.offsetMin = new Vector2(0.0f, 0.0f);
            buttonRightSide.offsetMax = new Vector2(0.0f, 0.0f);
            buttonRightSide.sizeDelta = new Vector2(0.0f, 0.0f);

            valueText!.anchorMin = new Vector2(0.0f, 0.0f);
            valueText.anchorMax = new Vector2(1.0f, 1.0f);
            valueText.offsetMin = new Vector2(0.0f, -0.33f);
            valueText.offsetMax = new Vector2(0.0f, 0.0f);
            valueText.sizeDelta = new Vector2(0.0f, 0.0f);

            var editIcon = buttonRightSide.Find("EditIcon").GetComponent<ImageView>();
            editIcon.sprite = Sprites.Pencil;
            editIcon.transform.localScale = new Vector3(-1.0f, -1.0f, 1.0f);
        }
    }
}