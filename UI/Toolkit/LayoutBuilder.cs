using HMUI;
using Zenject;

namespace ServerBrowser.UI.Toolkit
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LayoutBuilder
    {
        [Inject] private readonly DiContainer _diContainer = null!;

        public ViewController? ViewController { get; private set; }
        public LayoutContainer? Root { get; private set; }
        public LayoutContainer? Container { get; private set; }

        public LayoutContainer Init(ViewController viewController)
        {
            ViewController = viewController;
            Root = new LayoutContainer(this, viewController.transform, false);
            Container = Root.AddVerticalLayoutGroup("Container");
            return Container;
        }

        internal T CreateComponent<T>() where T : LayoutComponent
            => _diContainer.Instantiate<T>();
    }
}