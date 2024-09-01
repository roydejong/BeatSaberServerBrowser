using System;
using System.Collections.Generic;
using HMUI;
using ServerBrowser.Data;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Browser.Views
{
    public class MasterServerSelectViewController : ViewController, IInitializable
    {
        [Inject] private readonly DiContainer _diContainer = null!;
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;
        [Inject] private readonly MasterServerRepository _masterServerRepository = null!;

        private Dictionary<string, TkMasterServerCell> _cells = new();

        public event Action FinishedEvent;
        
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
            scrollRoot.SetScrollPerCellHeight(9f);
            
            var content = scrollRoot.Content!;
            content.InsertMargin(-1f, 4f);
            
            foreach (var masterServer in _masterServerRepository.All)
            {
                var serverTile = content.AddMasterServerCell();
                serverTile.SetData(masterServer);
                serverTile.ClickedEvent += HandleMasterServerClicked;
                _cells[masterServer.GraphUrl] = serverTile;
            }

            var bottomHorizontal = verticalSplit.AddHorizontalLayoutGroup("BottomControls");
            bottomHorizontal.PreferredWidth = 90f;
            bottomHorizontal.PreferredHeight = 10f;
            
            var applyButton = bottomHorizontal.AddButton("Select server", true,
                preferredWidth: 40f, preferredHeight: 13f);
            applyButton.AddClickHandler(() => FinishedEvent?.Invoke());

            UpdateSelectionState();
        }

        private void HandleMasterServerClicked(MasterServerRepository.MasterServerInfo obj)
        {
            _masterServerRepository.SelectedMasterServer = obj;
            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            var selectedServerUrl = _masterServerRepository.SelectedMasterServer.GraphUrl;
            foreach (var cellKv in _cells)
                cellKv.Value.SetIsSelected(cellKv.Key == selectedServerUrl);
        }
    }
}