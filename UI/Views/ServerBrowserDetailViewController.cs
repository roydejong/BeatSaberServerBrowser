using HMUI;
using ServerBrowser.UI.Components;

namespace ServerBrowser.UI.Views
{
    public class ServerBrowserDetailViewController : ViewController
    {
        public void Awake()
        {
            var ldc = LevelBarClone.Create(transform);
            ldc.TitleText = "Game server name, which may be long";
            ldc.SecondaryText = "BeatTogether Dedicated Server";
        }

        public void SetData(object serverInfo)
        {
            
        }
    }
}