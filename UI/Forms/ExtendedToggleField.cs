using System;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Tags;
using UnityEngine;

namespace ServerBrowser.UI.Forms
{
    public class ExtendedToggleField : ExtendedField<bool>
    {
        private bool _value;
        private readonly ToggleSetting _toggleSetting;

        public override bool Visible
        {
            get => _toggleSetting.gameObject.activeSelf;
            set => _toggleSetting.gameObject.SetActive(value);
        }

        public string Label
        {
            get => _toggleSetting.text.text;
            set => _toggleSetting.text.SetText(value);
        }

        public override bool Value
        {
            get => _value;
            set
            {
                _value = value;
                _toggleSetting.toggle.isOn = value;
            }
        }
        
        public override event EventHandler<bool>? OnChange;
        
        public ExtendedToggleField(Transform parent, string label, bool initialValue)
        {
            // Base
            var toggleTagObj = (new ToggleSettingTag()).CreateObject(parent);
            (toggleTagObj.transform as RectTransform)!.sizeDelta = new Vector2(90.0f, 7.0f);
            _toggleSetting = toggleTagObj.GetComponent<ToggleSetting>();

            // Label
            _toggleSetting.text.SetText(label);

            // Value
            _toggleSetting.toggle.isOn = initialValue;

            // Event
            _toggleSetting.toggle.onValueChanged.RemoveAllListeners();
            _toggleSetting.toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                _toggleSetting.toggle.isOn = newValue;
                OnChange?.Invoke(this, newValue);
            });
        }
    }
}