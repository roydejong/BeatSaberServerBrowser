using BeatSaberMarkupLanguage.Components;
using HMUI;
using ServerBrowser.Models;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class BssbPlayersTableRow : MonoBehaviour
    {
        private ImageView _bg = null!;
        private ImageView _icon = null!;
        private FormattableText _nameText = null!;
        private FormattableText _secondaryText = null!;
        
        public void Awake()
        {
            _bg = GetComponent<ImageView>();
            _icon = transform.GetChild(0).Find("BSMLImage").GetComponent<ImageView>();
            _nameText = transform.GetChild(1).Find("BSMLText").GetComponent<FormattableText>();
            _secondaryText = transform.GetChild(2).Find("BSMLText").GetComponent<FormattableText>();
            
            // Disable raycast target for our images so it doesn't cover other UI when scrolling
            _bg.raycastTarget = false;
            _icon.raycastTarget = false;
        }
        
        public void SetData(BssbPlayer player)
        {
            // TODO Icon
            // TODO Secondary text
            
            _nameText.SetText(player.UserName);
            _secondaryText.SetText("😀");
        }
    }
}