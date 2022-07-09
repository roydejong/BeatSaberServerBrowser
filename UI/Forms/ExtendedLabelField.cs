using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using TMPro;
using UnityEngine;

namespace ServerBrowser.UI.Forms
{
    public class ExtendedLabelField : ExtendedField
    {
        private readonly FormattableText _labelText;

        public override bool Visible
        {
            get => _labelText.gameObject.activeSelf;
            set => _labelText.gameObject.SetActive(value);
        }

        public string Label
        {
            get => _labelText.text;
            set
            {
                _labelText.text = value;
                _labelText.RefreshText();
            }
        }

        public ExtendedLabelField(Transform parent, string label)
        {
            var textTagObject = (new TextTag()).CreateObject(parent);
            
            _labelText = textTagObject.GetComponent<FormattableText>();
            _labelText.text = label;
            _labelText.rectTransform.offsetMin = new Vector2(0.0f, -30.0f);
            _labelText.rectTransform.offsetMax = new Vector2(90.0f, -30.0f);
            _labelText.rectTransform.sizeDelta = new Vector2(90.0f, 15.0f);
            _labelText.alignment = TextAlignmentOptions.Center;
            _labelText.fontSize = 4f;
            _labelText.extraPadding = true;
            _labelText.RefreshText();
        }
    }
}