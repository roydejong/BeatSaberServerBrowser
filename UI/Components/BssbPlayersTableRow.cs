using BeatSaberMarkupLanguage.Components;
using HMUI;
using ServerBrowser.Models;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class BssbPlayersTableRow : MonoBehaviour
    {
        private ImageView _icon = null!;
        private FormattableText _nameText = null!;
        private FormattableText _secondaryText = null!;
        
        public void Awake()
        {
            _icon = transform.GetChild(0).Find("BSMLImage").GetComponent<ImageView>();
            _nameText = transform.GetChild(1).Find("BSMLText").GetComponent<FormattableText>();
            _secondaryText = transform.GetChild(2).Find("BSMLText").GetComponent<FormattableText>();
        }
        
        public void SetData(BssbPlayer player)
        {
            _icon.color = Color.red;
            _nameText.SetText(player.UserName);
            _secondaryText.SetText(player.UserId);
        }
    }
}