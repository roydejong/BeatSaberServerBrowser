using System;
using JetBrains.Annotations;
using ServerBrowser.Data;
using ServerBrowser.Models;
using ServerBrowser.UI.Toolkit.Components;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Forms
{
    [UsedImplicitly]
    public class CreateServerFormExtender : FormExtender, IInitializable, IDisposable
    {
        [Inject] private readonly BssbConfig _config = null!;
        [Inject] private readonly MasterServerRepository _masterServerRepository = null!;
        [Inject] private readonly CreateServerViewController _createServerViewController = null!;
        
        private TkToggleControl _togglePublic = null!;
        private TkToggleControl _togglePpModifiers = null!;
        private TkToggleControl _togglePpDifficulties = null!;
        private TkToggleControl _togglePpMaps = null!;
        
        public void Initialize()
        {
            base.Initialize(_createServerViewController);

            _togglePublic = AddToggle("Add to Server Browser", _config.ToggleAnnounce);
            _togglePpModifiers = AddToggle("Per-player modifiers", _config.TogglePpModifiers);
            _togglePpDifficulties = AddToggle("Per-player difficulties", _config.TogglePpDifficulties);
            _togglePpMaps = AddToggle("Per-player maps", _config.TogglePpMaps);
            
            _createServerViewController.didActivateEvent += HandleViewActivated;
        }
        
        public new void Dispose()
        {
            base.Dispose();
            
            _createServerViewController.didActivateEvent -= HandleViewActivated;
        }

        private void HandleViewActivated(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            var masterServer = _masterServerRepository.SelectedMasterServer;
            
            _togglePpModifiers.GameObject.SetActive(masterServer.SupportsPpModifiers);
            _togglePpDifficulties.GameObject.SetActive(masterServer.SupportsPpDifficulties);
            _togglePpMaps.GameObject.SetActive(masterServer.SupportsPpMaps);
            
            MarkLayoutDirty();
        }

        private TkToggleControl AddToggle(string label, bool initialValue = false)
        {
            var toggle = Container.AddToggleControl(label, initialValue);
            
            var transform = (toggle.GameObject.transform as RectTransform)!;
            transform.SetSiblingIndex(NextSiblingIndex);
            transform.sizeDelta = new Vector2(90.0f, 7.0f);
            
            return toggle;
        }
    }
}