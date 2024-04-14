using System;
using UnityEngine;

namespace ServerBrowser.UI.Toolkit.Modals
{
    public abstract class TkModalView : MonoBehaviour
    {
        public abstract float ModalWidth { get; }
        public abstract float ModalHeight { get; }

        public TkModalHost ModalHost { get; set; } = null!;

        public event Action? ModalClosed;
        
        public abstract void Initialize();

        public void CloseModal() => ModalHost.CloseModal();
        public void InvokeModalClosed() => ModalClosed?.Invoke();
    }
}