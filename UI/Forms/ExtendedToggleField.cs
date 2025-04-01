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

        public override bool Value
        {
            get => _value;
            set
            {
                _value = value;
                _toggleSetting.Toggle.isOn = value;
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
            _toggleSetting.TextMesh.SetText(label);

            // Value
            _toggleSetting.Toggle.isOn = initialValue;

            // Event
            _toggleSetting.Toggle.onValueChanged.RemoveAllListeners();
            _toggleSetting.Toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                _toggleSetting.Toggle.isOn = newValue;
                OnChange?.Invoke(this, newValue);
            });
        }
    }
}