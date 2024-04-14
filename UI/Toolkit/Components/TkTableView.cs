using System.Collections.Generic;
using JetBrains.Annotations;
using ServerBrowser.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkTableView : LayoutComponent
    {
        public override GameObject GameObject => _goTable;
        
        private GameObject _goTable = null!;
        private RectTransform _rectTable = null!;
        private List<GameObject> _rowObjects = null!;
        private LayoutContainer _tableContainer = null!;
        private List<TableRow> _rowData = null!;

        public const float RowHeight = 5.4f;
        public const float FontSize = 3f;
        
        public override void AddToContainer(LayoutContainer container)
        {
            _goTable = new GameObject("TkTableView")
            {
                layer = LayoutContainer.UiLayer
            };
            _goTable.transform.SetParent(container.Transform, false);

            var vLayout = _goTable.AddComponent<VerticalLayoutGroup>();
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;
            
            _rectTable = _goTable.GetOrAddComponent<RectTransform>();
            _rectTable.anchorMin = new Vector2(0, 1);
            _rectTable.anchorMax = new Vector2(1, 1);
            
            _tableContainer = new LayoutContainer(container.Builder, _goTable.transform, true);

            _rowObjects = new();
            _rowData = new();
        }

        public TableRow AddRow(string label, string value = "")
            => AddRow(new TableRow { Label = label, Value = value });

        public TableRow AddRow(TableRow? row = null)
        {
            if (_rowObjects.Count >= 1)
            {
                var sep = _tableContainer.AddHorizontalLine();
                sep.SetGradient();
            }

            row ??= new TableRow();
            
            var goRow = new GameObject("RowRowRowTheBoat")
            {
                layer = LayoutContainer.UiLayer
            };
            goRow.transform.SetParent(_tableContainer.Transform, false);
            
            var layoutElement = goRow.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = RowHeight;
            layoutElement.minHeight = RowHeight;
            layoutElement.preferredWidth = -1f;
            
            var hLayout = goRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;
            
            var rowContainer = new LayoutContainer(_tableContainer.Builder, goRow.transform, false);
            row.LabelRef = rowContainer.AddText(row.Label, textAlignment: TextAlignmentOptions.MidlineLeft,
                fontSize: FontSize, color: BssbColors.InactiveGray);
            row.ValueRef = rowContainer.AddText(row.Value, textAlignment: TextAlignmentOptions.MidlineRight,
                fontSize: FontSize);

            _rowData.Add(row);
            _rowObjects.Add(goRow);

            return row;
        }

        public class TableRow
        {
            private string _label = "!Label!";
            private string _value = "!Value!";

            public TkText? LabelRef = null;
            public TkText? ValueRef = null;

            public string Label
            {
                get => _label;
                set
                {
                    _label = value;
                    LabelRef?.SetText(value);
                }
            }
            
            public string Value
            {
                get => _value;
                set
                {
                    _value = value;
                    ValueRef?.SetText(value);
                }
            }
        }
    }
}