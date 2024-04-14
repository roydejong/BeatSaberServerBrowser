using System;
using BGLib.Polyglot;
using HMUI;
using JetBrains.Annotations;
using ServerBrowser.Util;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkToggleControl : LayoutComponent
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly CloneHelper _cloneHelper = null!;

        private GameObject _toggleRoot = null!;
        private CurvedTextMeshPro _label = null!;
        private ToggleWithCallbacks _toggle = null!;

        public Action<bool>? ToggledEvent;

        public override void AddToContainer(LayoutContainer container)
        {
            _toggleRoot = _cloneHelper.CloneTemplate(
                _cloneHelper.GetToggleTemplate(), container.Transform, "TkToggleControl"
            );
            
            _label = _toggleRoot.transform.Find("NameText").GetComponent<CurvedTextMeshPro>();
            _label.gameObject.TryRemoveComponent<LocalizedTextMeshProUGUI>();
            _label.SetText("Label");
            
            _toggle = _toggleRoot.transform.Find("SwitchView").GetComponent<ToggleWithCallbacks>(); 
            _toggle.InstantClearState();

            _toggle.onValueChanged.AddListener(value => ToggledEvent?.Invoke(value));
            
            _toggleRoot.SetActive(true);
        }

        public void SetLabel(string label)
        {
            _label.SetText(label);
        }

        public void SetValue(bool value)
        {
            _toggle.isOn = value;
        }
    }
}