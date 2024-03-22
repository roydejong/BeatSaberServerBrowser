using JetBrains.Annotations;
using ServerBrowser.UI.Toolkit.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit.Components
{
    [UsedImplicitly]
    public class TkVerticalLayoutScrollView : TkScrollView
    {
        public override void AddToContainer(LayoutContainer container)
        {
            base.AddToContainer(container);
            
            var layoutGroup = Content!.GameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            
            var contentSizeFitter = Content.GameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            // Auto resize on content rect change
            var resizeListener = Content.GameObject.AddComponent<ScrollViewRectChangeListener>();
            resizeListener.BindScrollView(_scrollView!);
        }
    }
}