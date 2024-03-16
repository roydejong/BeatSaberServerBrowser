using UnityEngine;

namespace ServerBrowser.UI.Toolkit.Modals
{
    public abstract class TkModalView : MonoBehaviour
    {
        public abstract float ModalWidth { get; }
        public abstract float ModalHeight { get; }
        
        public abstract void Initialize();
    }
}