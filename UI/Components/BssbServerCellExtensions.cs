using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
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
            // Components
            _coverImage = transform.Find("CoverImage").GetComponent<ImageView>();
            _songName = _cell!.transform.Find("SongName").GetComponent<CurvedTextMeshPro>();
            _songAuthor = _cell.transform.Find("SongAuthor").GetComponent<CurvedTextMeshPro>();
            _favoritesIcon = _cell.transform.Find("FavoritesIcon").GetComponent<ImageView>();
            _songTime = _cell.transform.Find("SongTime").GetComponent<CurvedTextMeshPro>();
            _songBpm = _cell.transform.Find("SongBpm").GetComponent<CurvedTextMeshPro>();
            _bpmIcon = _cell.transform.Find("BpmIcon").GetComponent<ImageView>();
            
            // Events
            _cell!.selectionDidChangeEvent += HandleCellSelectionChange;
        }
        
        private void HandleCellSelectionChange(SelectableCell x, SelectableCell.TransitionType y, object z)
        {
            RefreshContent();
        }

        private void RefreshContent()
        {
            if (_server is null)
                return;
            
            // Re-align BPM text and allow more horizontal space - we use this for extended lobby type
            if (_songBpm != null)
            {
                var songBpmTransform = (_songBpm.transform as RectTransform)!;
                songBpmTransform.anchorMax = new Vector2(1.03f, 0.5f);
                songBpmTransform.offsetMin = new Vector2(-32.00f, -4.60f);
            }

            // Limit text size for server name and song name
            if (_songName != null)
                (_songName.transform as RectTransform)!.anchorMax = new Vector2(0.8f, 0.5f);
            
            if (_songAuthor != null)
                (_songAuthor.transform as RectTransform)!.anchorMax = new Vector2(0.8f, 0.5f);

            // Allow bigger player count size (just in case we get those fat 127/127 lobbies)
            if (_songTime != null)
                (_songTime.transform as RectTransform)!.offsetMin = new Vector2(-13.0f, -2.3f);
            
            // Enable rich text for subtext
            if (_songAuthor != null)
                _songAuthor.richText = true;
                
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
                else if (_server.IsLocallyDiscovered)
                    _songBpm.color = BssbColorScheme.Red;
                else if (_server.IsOfficial)
                    _songBpm.color = BssbColorScheme.Gold;
                else if (_server.IsBeatTogetherHost)
                    _songBpm.color = BssbColorScheme.Green;
                else
                    _songBpm.color = BssbColorScheme.Blue;
            }
            
            // Set default/fallback icon if unset or BSML did some bullshit
            if (_cellInfo != null && (_cellInfo.Icon == null || _cellInfo.Icon == Utilities.ImageResources.BlankSprite)) 
                _cellInfo.Icon = Sprites.PortalUser;
            if (_coverImage != null && (_coverImage.sprite == null || _coverImage.sprite == Utilities.ImageResources.BlankSprite))
                _coverImage.sprite = Sprites.PortalUser;
        }

        public async Task SetCoverArt(CancellationToken token)
        {
            if (_cell == null || _cellInfo == null || _server == null || _coverImage == null)
                return;
            
            try
            {
                var setSprite = Sprites.PortalUser;

                if (_server.IsLocallyDiscovered)
                {
                    // Local discovery server, we don't have level details so can't ever show cover art
                    // Use special icon specifically for these servers
                    setSprite = Sprites.SocialNetwork; 
                }
                else if (_server.Level is null)
                {
                    // Not level data, show new lobby icon
                }
                else
                {
                    // Playing level, show cover art
                    setSprite = await _coverArtLoader.FetchCoverArtAsync(
                        new CoverArtLoader.CoverArtRequest(_server, token)
                    );

                    if (setSprite == null)
                    {
                        setSprite = Sprites.BeatSaverLogo;
                    }
                }
                
                // Keep icon in sync so BSML doesn't clear it on scroll
                _cellInfo.Icon = setSprite;
                _coverImage.sprite = setSprite;
            }
            catch (Exception)
            {
                // Intentionally suppressing Unity errors that can happen here due to async loads
            }
        }
    }
}