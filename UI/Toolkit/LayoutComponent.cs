using System.Linq;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Toolkit
{
    public abstract class LayoutComponent 
    {
        [Inject] protected readonly MenuShockwave _shockwaveEffect = null!;
        
        protected static BasicUIAudioManager? BasicUIAudioManager;
        
        public abstract GameObject GameObject { get; }
        
        public abstract void AddToContainer(LayoutContainer container);

        protected void TriggerButtonClickEffect()
        {
            if (BasicUIAudioManager == null)
            {
                // Unfortunately this is not in the DI container and no one holds a reference to it
                BasicUIAudioManager = Resources.FindObjectsOfTypeAll<BasicUIAudioManager>().FirstOrDefault();
            }
            
            if (BasicUIAudioManager != null)
                BasicUIAudioManager.HandleButtonClickEvent();
            
            _shockwaveEffect.HandleButtonClickEvent();
        }
    }
}