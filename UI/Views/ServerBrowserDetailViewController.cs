using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Views
{
    public class ServerBrowserDetailViewController : ViewController
    {
        public void Awake()
        {
            var rootContainer = new GameObject("testRoot");
            rootContainer.transform.SetParent(transform);
            rootContainer.AddComponent<VerticalLayoutGroup>();
            
            var ldc = LevelBarClone.Create(rootContainer.transform);
            ldc.SetBackgroundStyle(LevelBarClone.BackgroundStyle.Test);
            ldc.SetImageSprite(Sprites.Pencil);
            ldc.SetText("Primary", "Secondary");
            
            var ldc2 = LevelBarClone.Create(rootContainer.transform);
            ldc2.SetBackgroundStyle(LevelBarClone.BackgroundStyle.GrayTitle);
            ldc2.SetImageSprite(Sprites.Pencil);
            ldc2.SetText("Primary2", "Secondary2");
        }

        public void SetData(object serverInfo)
        {
            
        }
    }
}