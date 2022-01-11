using System;
using System.Diagnostics.CodeAnalysis;
using IPA.Config;
using ServerBrowser.UI.Forms;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI
{
    public class CreateServerExtender : IInitializable, IAffinity
    {
        [Inject] private CreateServerViewController _viewController = null!;

        private Transform _wrapper = null!;
        private Transform _formView = null!;
        
        private FormExtender? _formExtender;
        private ExtendedStringField? _serverNameField;
        
        public void Initialize()
        {
            Plugin.Log?.Critical("CreateServerExtender -> Initialize");
            
            _wrapper = _viewController.transform.Find("Wrapper");
            _formView = _wrapper.transform.Find("CreateServerFormView");
        }
        
        [AffinityPostfix]
        [AffinityPatch(typeof(CreateServerViewController), "DidActivate")]
        private void HandleViewDidActivate(bool firstActivation)
        {
            if (!firstActivation)
                return;
            
            Plugin.Log?.Critical("CreateServerExtender -> DidActivate");

            _formExtender = new FormExtender(_formView);
            _serverNameField = _formExtender.CreateTextInput("Server Name", "Some name");
        }
        
        [AffinityPostfix]
        [AffinityPatch(typeof(CreateServerViewController), "ApplyAndGetData")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private void HandleFormCompletion(ref CreateServerFormData __result)
        {
            // TODO Setting check
            
            // If we are announcing the game, change form data to make the game "Public" on the master server
            // BT may use this in the future to announce from the server side
            
            __result.netDiscoverable = true;
            __result.allowInviteOthers = true;
            __result.usePassword = false;
        }
    }
}