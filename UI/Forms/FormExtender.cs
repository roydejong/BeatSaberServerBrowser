using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Forms
{
    public class FormExtender : MonoBehaviour
    {
        private readonly List<ExtendedField> _fields;
        private RectTransform _rectTransform;
        private RectTransform _rectTransformParent;
        private bool _innerLayoutDirty;
        private bool _outerLayoutDirty;

        public FormExtender()
        {
            _fields = new();
            _rectTransform = GetComponent<RectTransform>();
            _rectTransformParent = transform.parent.GetComponent<RectTransform>();
            _innerLayoutDirty = true;
            _outerLayoutDirty = true;
        }

        #region Layout update

        public void Update()
        {
            if (_innerLayoutDirty)
            {
                _innerLayoutDirty = false;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
                return;
            }

            if (_outerLayoutDirty)
            {
                _outerLayoutDirty = false;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransformParent);
                return;
            }
        }

        public void MarkDirty()
        {
            var formViewVerticalLayout = transform.GetComponent<VerticalLayoutGroup>();
            var parentVerticalLayout = transform.parent.GetComponent<VerticalLayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();

            if (formViewVerticalLayout != null && !formViewVerticalLayout.enabled)
                formViewVerticalLayout.enabled = true;

            if (contentSizeFitter != null && !contentSizeFitter.enabled)
                contentSizeFitter.enabled = true;
            
            if (parentVerticalLayout != null && !parentVerticalLayout.enabled)
                parentVerticalLayout.enabled = true;
            
            _innerLayoutDirty = true;
            _outerLayoutDirty = true;
        }
        
        #endregion

        #region Fields API

        public ExtendedStringField CreateTextInput(string label, string? initialValue)
        {
            var field = new ExtendedStringField(transform, label, initialValue);
            _fields.Add(field);
            MarkDirty();
            return field;
        }
        
        public ExtendedToggleField CreateToggleInput(string label, bool initialValue)
        {
            var field = new ExtendedToggleField(transform, label, initialValue);
            _fields.Add(field);
            MarkDirty();
            return field;
        }
        
        public ExtendedLabelField CreateLabel(string label)
        {
            var field = new ExtendedLabelField(transform, label);
            _fields.Add(field);
            MarkDirty();
            return field;
        }

        #endregion
    }
}