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

        private CurvedTextMeshPro SongName;
        private CurvedTextMeshPro SongAuthor;
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

                SongName = _cell.transform.Find("SongName").GetComponent<CurvedTextMeshPro>();
                SongAuthor = _cell.transform.Find("SongAuthor").GetComponent<CurvedTextMeshPro>();
                FavoritesIcon = _cell.transform.Find("FavoritesIcon").GetComponent<ImageView>();
                SongTime = _cell.transform.Find("SongTime").GetComponent<CurvedTextMeshPro>();
                SongBpm = _cell.transform.Find("SongBpm").GetComponent<CurvedTextMeshPro>();
                BpmIcon = _cell.transform.Find("BpmIcon").GetComponent<ImageView>();

                // Re-align BPM text and allow more horizontal space - we use this for extended lobby type
                var songBpmTransform = SongBpm.transform as RectTransform;
                songBpmTransform.anchorMax = new Vector2(1.03f, 0.5f);
                songBpmTransform.offsetMin = new Vector2(-32.00f, -4.60f);

                // Limit text size for server name and song name
                (SongName.transform as RectTransform).anchorMax = new Vector2(0.8f, 0.5f);
                (SongAuthor.transform as RectTransform).anchorMax = new Vector2(0.8f, 0.5f);

                // Allow bigger player count size (just in case we get those fat 100/100 lobbies)
                (SongTime.transform as RectTransform).offsetMin = new Vector2(-13.0f, -2.3f);

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

        public void RefreshContent(HostedGameCellData cellInfo = null)
        {
            if (cellInfo != null)
                _cellInfo = cellInfo;

            FavoritesIcon.gameObject.SetActive(false);
            SongTime.gameObject.SetActive(true);
            SongBpm.gameObject.SetActive(true);
            BpmIcon.gameObject.SetActive(false);

            var game = _cellInfo.Game;

            // Player count
            SongTime.text = $"{game.PlayerCount}/{game.PlayerLimit}";
            SongTime.color = (game.PlayerCount >= game.PlayerLimit ? Color.gray : Color.white);
            SongTime.fontSize = 4;

            // Lobby type (server + modded/vanilla indicator)
            SongBpm.text = game.DescribeType();
            SongBpm.color = (game.IsModded ? Color.cyan : Color.green);
        }
        #endregion
    }
}
