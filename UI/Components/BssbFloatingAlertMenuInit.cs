using Zenject;

namespace ServerBrowser.UI.Components
{
    /// <summary>
    /// Helps the global BssbFloatingAlert set up at menu time.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbFloatingAlertMenuInit : IInitializable
    {
        [Inject] private readonly BssbFloatingAlert _floatingAlert = null!;
        
        #region Lifecycle
        public void Initialize()
        {
            _floatingAlert.OnMenu();
        }
        #endregion
    }
}