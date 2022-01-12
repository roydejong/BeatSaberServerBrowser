using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using ServerBrowser.UI.Components;
using UnityEngine.UI;

namespace ServerBrowser.UI.Views
{
    [HotReload]
    public class ServerBrowserDetailViewController : BSMLAutomaticViewController
    {
        [UIComponent("mainContentRoot")] private VerticalLayoutGroup _mainContentRoot;
        [UIComponent("titleBarRoot")] private VerticalLayoutGroup _titleBarRoot;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            var ldc = LevelBarClone.Create(_titleBarRoot.transform);
            ldc.SetImageVisible(true);
            ldc.SetBackgroundStyle(LevelBarClone.BackgroundStyle.Test);
            ldc.SetText("!Server name!", "!Server type!");
        }

        public void SetData(object serverInfo)
        {
            
        }
    }
}