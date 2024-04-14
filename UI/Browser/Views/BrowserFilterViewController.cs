using System;
using System.Collections.Generic;
using HMUI;
using ServerBrowser.Models;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public class BrowserFilterViewController : ViewController, IInitializable
    {
        [Inject] private readonly DiContainer _diContainer = null!;
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;

        private ServerFilterParams _filterParams = null!;
        private Dictionary<string, TkToggleControl> _toggleControls = new();
        public event Action<ServerFilterParams?> FinishedEvent;

        public void Init(ServerFilterParams filterParams)
        {
            _filterParams = filterParams;

            foreach (var toggleKv in _toggleControls)
                toggleKv.Value.SetValue(_filterParams.GetValue(toggleKv.Key));
        }
        
        public void Initialize()
        {
            BuildLayout(_layoutBuilder.Init(this));
        }
        
        private void BuildLayout(LayoutContainer root)
        {
            root.GameObject.TryRemoveComponent<ContentSizeFitter>();
            root.GameObject.TryRemoveComponent<LayoutElement>();
            var rootRect = root.GameObject.GetComponent<RectTransform>();
            rootRect.offsetMin = new Vector2(35f, -35f);
            rootRect.offsetMax = new Vector2(-35f, 0f);
            rootRect.sizeDelta = new Vector2(-70f, 35f);
            
            var verticalSplit = root.AddVerticalLayoutGroup("VerticalSplit",
                expandChildHeight: true, expandChildWidth: true, childAlignment: TextAnchor.MiddleCenter);
            verticalSplit.PreferredWidth = 90f;
            verticalSplit.PreferredWidth = 100f;
            
            var scrollRoot = verticalSplit.AddVerticalLayoutScrollView();
            
            var content = scrollRoot.Content!;
            content.InsertMargin(-1f, 6f);
            
            foreach (var param in ServerFilterParams.AllParams)
            {
                var toggle = param.CreateControl(content);
                toggle.ToggledEvent += value =>
                {
                    _filterParams.SetValue(param.Key, value);
                };
                _toggleControls[param.Key] = toggle;
            }

            var bottomHorizontal = verticalSplit.AddHorizontalLayoutGroup("BottomControls");
            bottomHorizontal.PreferredWidth = 90f;
            bottomHorizontal.PreferredHeight = 10f;
            
            var applyButton = bottomHorizontal.AddButton("Apply", true,
                preferredWidth: 40f, preferredHeight: 13f);
            applyButton.AddClickHandler(() =>
            {
                FinishedEvent?.Invoke(_filterParams);
            });
        }
    }
}