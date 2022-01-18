using System.Diagnostics.CodeAnalysis;
using ServerBrowser.Core;
using ServerBrowser.UI.Forms;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CreateServerExtender : IInitializable, IAffinity
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly ServerBrowserClient _bssbClient = null!;
        [Inject] private readonly CreateServerViewController _viewController = null!;

        private Transform _wrapper = null!;
        private Transform _formView = null!;

        private FormExtender? _formExtender;
        private ExtendedToggleField? _announcePartyField;
        private ExtendedStringField? _serverNameField;
        private ExtendedLabelField? _masterServerText;

        public void Initialize()
        {
            _wrapper = _viewController.transform.Find("Wrapper");
            _formView = _wrapper.transform.Find("CreateServerFormView");
            
            // Extend form
            _formExtender = new FormExtender(_formView);

            _announcePartyField = _formExtender.CreateToggleInput("Add to Server Browser", _config.AnnounceParty);
            _announcePartyField.OnChange += HandleAnnouncePartyChange;

            _serverNameField = _formExtender.CreateTextInput("Server Name", _config.ServerName);
            _serverNameField.OnChange += HandleServerNameChange;

            _masterServerText = _formExtender.CreateLabel("");
            
            // Set initial values
            UpdateForm();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(CreateServerViewController), "DidActivate")]
        private void HandleViewDidActivate(bool firstActivation)
        {
            UpdateForm();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(CreateServerViewController), "ApplyAndGetData")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private void HandleFormCompletion(ref CreateServerFormData __result)
        {
            if (!_config.AnnounceParty)
                // Announce disabled; do not modify
                return;

            __result.allowInviteOthers = true;
            __result.usePassword = false;
            
            // If we are announcing the game, change form data to make the game "Public" on the master server
            // BT may use this in the future to announce from the server side
            // We can not set this for Official Servers as it forces the player limit to 5
            __result.netDiscoverable = !_bssbClient.UsingOfficialMaster;
        }

        private void HandleAnnouncePartyChange(object sender, bool newValue)
        {
            _log.Info($"HandleAnnouncePartyChange: {newValue}");

            _config.AnnounceParty = newValue;

            UpdateForm();
        }

        private void HandleServerNameChange(object sender, string? newValue)
        {
            _log.Info($"HandleServerNameChange: {newValue}");

            _config.ServerName = newValue;

            UpdateForm();
        }

        private void UpdateForm()
        {
            if (_serverNameField is not null)
            {
                _serverNameField.Visible = _config.AnnounceParty;
                _serverNameField.Value = _bssbClient.PreferredServerName;
            }

            if (_masterServerText is not null)
            {
                string text;

                if (_bssbClient.UsingOfficialMaster)
                    text = $"<color=#fbc531>Creating lobby on Official Servers (custom songs NOT supported)";
                else if (_bssbClient.UsingBeatTogetherMaster)
                    text = $"<color=#4cd137>Creating lobby on BeatTogether (supports custom songs)";
                else
                    text = $"<color=#00a8ff>Creating lobby on custom master server: {_bssbClient.MasterServerHost}";

                _masterServerText.Label = text;
            }

            _formExtender?.RefreshVerticalLayout();
        }
    }
}