using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace ServerBrowser.UI
{
    [HotReload]
    public class ServerBrowserMainViewController : BSMLAutomaticViewController
    {
        public event EventHandler<EventArgs> OnCreateServerClicked;
    }
}