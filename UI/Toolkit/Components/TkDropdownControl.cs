using BGLib.Polyglot;
using HMUI;
using JetBrains.Annotations;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkDropdownControl : LayoutComponent
    {
        [Inject] private readonly CloneHelper _cloneHelper = null!;

        public override GameObject GameObject => _dropdownRoot;

        private GameObject _dropdownRoot = null!;
        private CurvedTextMeshPro _label = null!;
        private SimpleTextDropdown _dropdown = null!;

        public override void AddToContainer(LayoutContainer container)
        {
            _dropdownRoot = _cloneHelper.CloneTemplate(
                _cloneHelper.GetDropdownTemplate(), container.Transform, "TkDropdownControl"
            );
            
            _label = _dropdownRoot.transform.Find("Label").GetComponent<CurvedTextMeshPro>();
            _label.gameObject.TryRemoveComponent<LocalizedTextMeshProUGUI>();
            _label.SetText("Label");
            
            _dropdown = _dropdownRoot.transform.Find("SimpleTextDropDown").GetComponent<SimpleTextDropdown>();
            _dropdown.SetTexts(new[] { "Option 1", "Option 2", "Option 3" });
        }
        
        public void SetLabel(string text)
        {
            _label.SetText(text);
        }

        public void SetValueText(string valueText)
        {
            Plugin.Log.Info("SetValueText: " + valueText);
            _dropdown._text.SetText(valueText);
        }

        public void RemoveAllOnClickActions()
        {
            _dropdown._button.onClick.RemoveAllListeners();
        }

        public void AddOnClick(UnityAction action)
        {
            _dropdown._button.onClick.AddListener(action);
        }
    }
}