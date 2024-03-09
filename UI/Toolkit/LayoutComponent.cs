namespace ServerBrowser.UI.Toolkit
{
    public abstract class LayoutComponent 
    {
        public abstract void AddToContainer(LayoutContainer container);
        
        public abstract void SetActive(bool active);
    }
}