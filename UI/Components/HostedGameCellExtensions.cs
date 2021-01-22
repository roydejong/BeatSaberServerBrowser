using HMUI;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class HostedGameCellExtensions : MonoBehaviour
    {
        private bool _enabled = false;

        private SelectableCell _cell;
        private HostedGameCellData _cellInfo;

        private ImageView _background;

        private ImageView FavoritesIcon;
        private CurvedTextMeshPro SongTime;
        private CurvedTextMeshPro SongBpm;
        private ImageView BpmIcon;

        #region Setup
        internal void Configure(SelectableCell cell, HostedGameCellData cellInfo)
        {
            this._cell = cell;
            this._cellInfo = cellInfo;

            this._background = cell.transform.Find("Background").GetComponent<ImageView>();

            if (_enabled)
            {
                OnEnable();
            }
        }
        #endregion

        #region Events
        private void OnEnable()
        {
            if (_cell != null)
            {
                _cell.highlightDidChangeEvent += OnHighlightDidChange;
                _cell.selectionDidChangeEvent += OnSelectionDidChange;

                RefreshBackground();

                FavoritesIcon = _cell.transform.Find("FavoritesIcon").GetComponent<ImageView>();
                SongTime = _cell.transform.Find("SongTime").GetComponent<CurvedTextMeshPro>();
                SongBpm = _cell.transform.Find("SongBpm").GetComponent<CurvedTextMeshPro>();
                BpmIcon = _cell.transform.Find("BpmIcon").GetComponent<ImageView>();

                RefreshContent();
            }
            
            _enabled = true;
        }

        private void OnDisable()
        {
            _enabled = false;

            if (_cell != null)
            {
                _cell.highlightDidChangeEvent -= OnHighlightDidChange;
                _cell.selectionDidChangeEvent -= OnSelectionDidChange;
            }
        }

        private void OnSelectionDidChange(SelectableCell cell, SelectableCell.TransitionType transition, object _)
        {
            RefreshBackground();
            RefreshContent();
        }

        private void OnHighlightDidChange(SelectableCell cell, SelectableCell.TransitionType transition)
        {
            RefreshBackground();
        }
        #endregion

        #region Background
        private void RefreshBackground()
        {
            _background.color = new Color(192f / 255f, 43f / 255f, 180f / 255f, 1.0f);

            if (_cell.selected)
            {
                Plugin.Log.Info($"Selected cell test: {this._cellInfo.Game.GameName}");

                _background.color0 = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                _background.color1 = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                _background.enabled = true;
            }
            else if (_cell.highlighted)
            {
                _background.color0 = new Color(1.0f, 1.0f, 1.0f, 0.75f);
                _background.color1 = new Color(1.0f, 1.0f, 1.0f, 0.25f);
                _background.enabled = true;
            }
            else
            {
                _background.enabled = false;
            }
        }

        public void RefreshContent()
        {
            FavoritesIcon.gameObject.SetActive(false);
            SongTime.gameObject.SetActive(true);
            SongBpm.gameObject.SetActive(true);
            BpmIcon.gameObject.SetActive(false);

            var game = _cellInfo.Game;

            // Player count
            SongTime.text = $"{game.PlayerCount}/{game.PlayerLimit}";
            SongTime.color = (game.PlayerCount >= game.PlayerLimit ? Color.gray : Color.white);
            SongTime.fontSize = 4;

            // Vanilla / modded indicator
            SongBpm.text = game.DescribeType();
            SongBpm.color = (game.IsModded ? Color.cyan : Color.green);

            var songBpmTransform = SongBpm.transform as RectTransform;
            songBpmTransform.anchorMax = new Vector2(1.03f, 0.5f);
            songBpmTransform.offsetMin = new Vector2(-32.00f, -4.60f);
        }
        #endregion
    }
}
