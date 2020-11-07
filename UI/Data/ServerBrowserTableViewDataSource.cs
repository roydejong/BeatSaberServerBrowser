using HMUI;
using IPA.Utilities;
using ServerBrowser.Core;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Data
{
    class ServerBrowserTableViewDataSource : TableViewWithDetailCell.IDataSource
    {
        private List<HostedGameData> _cells;
        private GameServerListTableCell _cellPrefab;

        [Inject]
        protected readonly DiContainer _container;

        public ServerBrowserTableViewDataSource(GameServerListTableCell cellPrefab)
        {
            _cellPrefab = cellPrefab;
        }

        public void Feed(IEnumerable<HostedGameData> servers)
        {
            _cells = new List<HostedGameData>(servers);
        }

        public TableCell CellForIdx(TableView tableView, int idx)
        {
            var hostedGameData = _cells[idx];

            // Instantiate/reuse cell prefab
            var cellIdentifier = "Cell";
            var serverCell = tableView.DequeueReusableCellForIdentifier(cellIdentifier) as GameServerListTableCell;

            if (serverCell == null)
            {
                serverCell = this._container.InstantiatePrefab(_cellPrefab).GetComponent<GameServerListTableCell>();
                serverCell.reuseIdentifier = cellIdentifier;
            }

            SetCellData(serverCell, hostedGameData);

            Plugin.Log.Warn($"Made a nice server cell: {idx} / {hostedGameData.ServerCode} / {hostedGameData.GameName}");
            return serverCell;
        }

        private void SetCellData(GameServerListTableCell cell, HostedGameData data)
        {
            ReflectionUtil.GetField<CurvedTextMeshPro, GameServerListTableCell>(cell, "_serverName").SetText(data.GameName);
            ReflectionUtil.GetField<CurvedTextMeshPro, GameServerListTableCell>(cell, "_difficultiesText").SetText(data.GameName);
            ReflectionUtil.GetField<CurvedTextMeshPro, GameServerListTableCell>(cell, "_musicPackText").SetText(data.GameName);
            ReflectionUtil.GetField<CurvedTextMeshPro, GameServerListTableCell>(cell, "_playerCount").SetText(data.GameName);
            ReflectionUtil.GetField<GameObject, GameServerListTableCell>(cell, "_passwordProtected").SetActive(true); // lock icon
        }

        public float CellSize()
        {
            Plugin.Log.Warn($"CellSize: 10");
            return 10.0f;
        }

        public int NumberOfCells()
        {
            if (_cells != null)
            {
                Plugin.Log.Warn($"NumberOfCells: {_cells.Count}");
                return _cells.Count;
            }

            return 0;
        }

        public TableCell CellForContent(TableViewWithDetailCell tableView, int idx, bool detailOpened)
        {
            Plugin.Log.Warn($"CellForContent {idx}");
            return CellForIdx(tableView, idx);
        }

        public TableCell CellForDetail(TableViewWithDetailCell tableView, int contentIdx)
        {
            Plugin.Log.Warn($"CellForDetail {contentIdx}");
            return null;
        }
    }
}
