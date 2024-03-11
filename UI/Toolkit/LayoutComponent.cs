using System.Linq;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Toolkit
{
    public abstract class LayoutComponent 
    {
        [Inject] protected readonly MenuShockwave _shockwaveEffect = null!;
        
        protected BasicUIAudioManager? _basicUIAudioManager;
        
        public abstract void AddToContainer(LayoutContainer container);

        protected void TriggerButtonClickEffect()
        {
            if (_basicUIAudioManager == null)
            {
                // Unfortunately this is not in the DI container and no one holds a reference to it
                _basicUIAudioManager = Resources.FindObjectsOfTypeAll<BasicUIAudioManager>().FirstOrDefault();
            }
            
            if (_basicUIAudioManager != null)
                _basicUIAudioManager.HandleButtonClickEvent();
            
            _shockwaveEffect.HandleButtonClickEvent();
        }
    }
}