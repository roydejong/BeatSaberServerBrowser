using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using LobbyBrowserMod.Core;
using LobbyBrowserMod.UI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyBrowserMod.UI
{
    public class LobbyBrowserViewController : BeatSaberMarkupLanguage.ViewControllers.BSMLResourceViewController
    {
        public override string ResourceName => "LobbyBrowserMod.UI.LobbyBrowserViewController.bsml";

        [UIValue("lobby-options")]
        internal List<object> queueItems = new List<object>()
        {
            new LobbyUiItem(new LobbyAnnounceInfo()
            {
                GameName = "Game 1",
                IsModded = true,
                PlayerCount = 1,
                PlayerLimit = 3,
                ServerCode = "ABCDEF"
            }),
            new LobbyUiItem(new LobbyAnnounceInfo()
            {
                GameName = "Game 2",
                IsModded = true,
                PlayerCount = 5,
                PlayerLimit = 5,
                ServerCode = "ABCDEF"
            }),
            new LobbyUiItem(new LobbyAnnounceInfo()
            {
                GameName = "Game 3",
                IsModded = false,
                PlayerCount = 3,
                PlayerLimit = 5,
                ServerCode = "ABCDEF"
            }),
        };
    }
}