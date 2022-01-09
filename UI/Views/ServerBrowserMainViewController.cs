using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace ServerBrowser.UI.Views
{
    [HotReload]
    public class ServerBrowserMainViewController : BSMLAutomaticViewController
    {
        public event EventHandler<EventArgs>? CreateServerClickedEvent;
    }
}