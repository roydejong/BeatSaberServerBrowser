using System;
using System.Threading;
using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.Models;
using ServerBrowser.UI.Utils;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Components
{
    public class BssbServerCellExtensions : MonoBehaviour
    {
        [Inject] private readonly CoverArtLoader _coverArtLoader = null!;
        
        private TableCell? _cell = null;
        private BssbServerCellInfo? _cellInfo = null;
        private BssbServer? _server = null;

        private ImageView? _coverImage;
        private CurvedTextMeshPro? _songName;
        private CurvedTextMeshPro? _songAuthor;
        private ImageView? _favoritesIcon;
        private CurvedTextMeshPro? _songTime;
        private CurvedTextMeshPro? _songBpm;
        private ImageView? _bpmIcon;

        public void SetData(TableCell cell, BssbServerCellInfo cellInfo, BssbServer server)
        {
            if (_cell is null || _cell != cell)
            {
                _cell = cell;
                BindComponents();
            }
                
            _cellInfo = cellInfo;
            _server = server;
            
            RefreshContent();
        }

        private void BindComponents()
        {
            _coverImage = transform.Find("CoverImage").GetComponent<ImageView>();
            _songName = _cell!.transform.Find("SongName").GetComponent<CurvedTextMeshPro>();
            _songAuthor = _cell.transform.Find("SongAuthor").GetComponent<CurvedTextMeshPro>();
            _favoritesIcon = _cell.transform.Find("FavoritesIcon").GetComponent<ImageView>();
            _songTime = _cell.transform.Find("SongTime").GetComponent<CurvedTextMeshPro>();
            _songBpm = _cell.transform.Find("SongBpm").GetComponent<CurvedTextMeshPro>();
            _bpmIcon = _cell.transform.Find("BpmIcon").GetComponent<ImageView>();
            
            // Re-align BPM text and allow more horizontal space - we use this for extended lobby type
            var songBpmTransform = (_songBpm.transform as RectTransform)!;
            songBpmTransform.anchorMax = new Vector2(1.03f, 0.5f);
            songBpmTransform.offsetMin = new Vector2(-32.00f, -4.60f);

            // Limit text size for server name and song name
            (_songName.transform as RectTransform)!.anchorMax = new Vector2(0.8f, 0.5f);
            (_songAuthor.transform as RectTransform)!.anchorMax = new Vector2(0.8f, 0.5f);

            // Allow bigger player count size (just in case we get those fat 127/127 lobbies)
            (_songTime.transform as RectTransform)!.offsetMin = new Vector2(-13.0f, -2.3f);
            
            // Enable rich text for subtext
            _songAuthor.richText = true;
            
            // Events
            _cell.selectionDidChangeEvent += HandleCellSelectionChange;
        }
        
        private void HandleCellSelectionChange(SelectableCell x, SelectableCell.TransitionType y, object z)
        {
            RefreshContent();
        }

        private void RefreshContent()
        {
            if (_server is null)
                return;
            
            // Unused parts
            if (_favoritesIcon != null)
                _favoritesIcon.gameObject.SetActive(false);
            
            if (_bpmIcon != null)
                _bpmIcon.gameObject.SetActive(false);
            
            // Player count
            if (_songTime != null)
            {
                _songTime.gameObject.SetActive(true);
                _songTime.text = $"{_server.ReadOnlyPlayerCount}/{_server.PlayerLimit}";
                _songTime.fontSize = 4;
                _songTime.color = _server.ReadOnlyPlayerCount >= _server.PlayerLimit
                    ? BssbColorScheme.MutedGray
                    : BssbColorScheme.White;
            }

            // Lobby type
            if (_songBpm != null)
            {
                _songBpm.gameObject.SetActive(true);
                _songBpm.text = _server.ServerTypeText;

                if (_cell != null && _cell.selected)
                    _songBpm.color = BssbColorScheme.White;
                else if (_server.IsOfficial)
                    _songBpm.color = BssbColorScheme.Gold;
                else if (_server.IsBeatTogetherHost)
                    _songBpm.color = BssbColorScheme.Green;
                else
                    _songBpm.color = BssbColorScheme.Blue;
            }
        }

        public void SetCoverArt(CancellationToken token)
        {
            try
            {
                if (_cell == null || _cellInfo == null || _server == null || _coverImage == null)
                    return;

                if (_server.IsInLobby || _server.Level is null)
                {
                    // Not in level, show lobby icon
                    _coverImage.sprite = Sprites.PortalUser;
                    return;
                }
                
                // Playing level, show cover art
                _coverImage.sprite = Sprites.BeatSaverLogo;
                
                _coverArtLoader.FetchCoverArtAsync(new CoverArtLoader.CoverArtRequest(_server, token, sprite =>
                {
                    if (sprite != null)
                        _coverImage.sprite = sprite;
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}