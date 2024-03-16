using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Modals;
using TMPro;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Browser.Modals
{
    public class TestModalView : TkModalView
    {
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;

        public override float ModalWidth => 75f;
        public override float ModalHeight => 30f;

        public override void Initialize()
        {
            Plugin.Log.Error("TestModalView init!");
            
            var container = new LayoutContainer(_layoutBuilder, transform, false);
            var inner = container.AddVerticalLayoutGroup("Inner",
                padding: new RectOffset(1, 1, 1, 1),
                expandChildWidth: true, expandChildHeight: false);
            inner.SetBackground("round-rect-panel");
            inner.AddButton("hello world");
            inner.AddText("test 123", textAlignment: TextAlignmentOptions.Center);
        }
    }
}