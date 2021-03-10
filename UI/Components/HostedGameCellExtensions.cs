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

                SongName = _cell.transform.Find("SongName").GetComponent<CurvedTextMeshPro>();
                SongAuthor = _cell.transform.Find("SongAuthor").GetComponent<CurvedTextMeshPro>();
                FavoritesIcon = _cell.transform.Find("FavoritesIcon").GetComponent<ImageView>();
                SongTime = _cell.transform.Find("SongTime").GetComponent<CurvedTextMeshPro>();
                SongBpm = _cell.transform.Find("SongBpm").GetComponent<CurvedTextMeshPro>();
                BpmIcon = _cell.transform.Find("BpmIcon").GetComponent<ImageView>();

                SongAuthor.richText = true;

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
            RefreshContent();
        }

        private void OnHighlightDidChange(SelectableCell cell, SelectableCell.TransitionType transition)
        {
        }
        #endregion

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
            SongTime.fontSize = 4;

            // Lobby type (server + modded/vanilla indicator)
            SongBpm.text = game.DescribeType();

            // Text colors
            if (_cell.selected)
            {
                // Selected /w blue background: More contrasting color needed
                SongTime.color = Color.white;
                SongBpm.color = Color.white;
            }
            else
            {
                SongTime.color = (game.PlayerCount >= game.PlayerLimit ? Color.gray : Color.white);
                SongBpm.color = (game.IsModded ? new Color(143.0f / 255.0f, 72.0f / 255.0f, 219.0f / 255.0f, 1.0f)
                    : new Color(60.0f / 255.0f, 179.0f / 255.0f, 113.0f / 255.0f, 1.0f));
            }
            
        }
    }
}
