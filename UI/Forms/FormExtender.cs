using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Forms
{
    public class FormExtender
    {
        private readonly Transform _formView;
        private readonly List<ExtendedField> _fields;

        public FormExtender(Transform formView)
        {
            _formView = formView;
            _fields = new();
        }

        public void RefreshVerticalLayout()
        {
            var formViewVerticalLayout = _formView.GetComponent<VerticalLayoutGroup>();
            
            // Disable vertical layout
            formViewVerticalLayout.enabled = false;

            // Calculate extra height and modify size delta
            var extraHeight = 0f;
            
            foreach (var field in _fields)
                if (field.Visible)
                    extraHeight += field.Height;
            
            var rectTransform = (_formView as RectTransform)!;
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0.0f);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, extraHeight);

            // Re-apply vertical layout with new height
            formViewVerticalLayout.enabled = true;
        }
        
        public ExtendedStringField CreateTextInput(string label, string? initialValue)
        {
            var field = new ExtendedStringField(_formView, label, initialValue);
            _fields.Add(field);
            RefreshVerticalLayout();
            return field;
        }
        
        public ExtendedToggleField CreateToggleInput(string label, bool initialValue)
        {
            var field = new ExtendedToggleField(_formView, label, initialValue);
            _fields.Add(field);
            RefreshVerticalLayout();
            return field;
        }
    }
}