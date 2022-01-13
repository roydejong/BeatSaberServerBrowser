using System.Collections.Generic;
using ServerBrowser.Models;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class BssbPlayersTable
    {
        private GameObject _parent;
        private GameObject _rowPrefab;
        private Dictionary<int, BssbPlayersTableRow> _rows;
        private int _rowIterator;

        public BssbPlayersTable(GameObject parent, GameObject rowPrefab)
        {
            _parent = parent;
            _parent.name = "BssbPlayersTable";
            _rowPrefab = rowPrefab;
            _rows = new();
            _rowIterator = 0;
        }

        public void Clear()
        {
            foreach (var row in _rows.Values)
                row.gameObject.SetActive(false);

            _rowIterator = 0;
        }

        public void SetData(IEnumerable<BssbServerPlayer> playerList)
        {
            Clear();

            foreach (var player in playerList)
            {
                var row = GetNewRow();
                row.SetData(player);
            }
        }

        private BssbPlayersTableRow GetNewRow()
        {
            BssbPlayersTableRow row;

            if (_rows.ContainsKey(_rowIterator))
            {
                row = _rows[_rowIterator];
            }
            else
            {
                var go = Object.Instantiate(_rowPrefab, _parent.transform);
                go.name = $"BssbPlayersTableRow{_rowIterator}";
                go.transform.SetSiblingIndex(_rowIterator);
                
                row = go.AddComponent<BssbPlayersTableRow>();
            }
            
            row.gameObject.SetActive(true);
            _rowIterator++;
            return row;
        }

    }
}