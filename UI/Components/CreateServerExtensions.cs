using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.Tags.Settings;
using HMUI;
using ServerBrowser.Game;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Components
{
    public class CreateServerExtensions : MonoBehaviour
    {
        private Transform _wrapper;
        private Transform _formView;
        private StringSetting _serverNameSetting;

        public void Awake()
        {
            _wrapper = transform.Find("Wrapper");
            _formView = _wrapper.transform.Find("CreateServerFormView");

            CreateToggle("Add to Server Browser", AddToBrowserValue, OnAddToBrowserChange);
            _serverNameSetting = CreateTextInput("Server Name", ServerNameValue, OnServerNameChange);
        }

        public void OnEnable()
        {
            OnAddToBrowserChange(AddToBrowserValue);
            OnServerNameChange(ServerNameValue);
        }

        #region UI Helpers
        private ToggleSetting CreateToggle(string label, bool value, Action<bool> onChangeCallback, string hoverHint = null)
        {
            // Base
            var toggleTagObj = (new ToggleSettingTag()).CreateObject(_formView);
            (toggleTagObj.transform as RectTransform).sizeDelta = new Vector2(90.0f, 7.0f);
            var toggleSetting = toggleTagObj.GetComponent<ToggleSetting>();

            // Label
            toggleSetting.text.SetText(label);

            // Value
            toggleSetting.toggle.isOn = value;

            // Event
            toggleSetting.toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                onChangeCallback(newValue);
            });

            return toggleSetting;
        }

        private StringSetting CreateTextInput(string label, string value, Action<string> onChangeCallback)
        {
            // Base
            var stringTagObj = (new StringSettingTag()).CreateObject(_formView);
            (stringTagObj.transform as RectTransform).sizeDelta = new Vector2(90.0f, 7.0f);
            var stringSetting = stringTagObj.GetComponent<StringSetting>();

            // Label
            stringSetting.GetComponentInChildren<TextMeshProUGUI>().text = label;

            // Value
            stringSetting.modalKeyboard.clearOnOpen = false;
            stringSetting.modalKeyboard.keyboard.KeyboardText.text = value;
            stringSetting.text.text = value;
            stringSetting.text.richText = false;

            // Event
            stringSetting.modalKeyboard.keyboard.EnterPressed += (delegate (string newValue)
            {
                onChangeCallback(newValue);
            });

            return stringSetting;
        }

        private void ReApplyVerticalLayout(bool extraHeight)
        {
            _wrapper.GetComponent<VerticalLayoutGroup>().enabled = false;
            _formView.GetComponent<VerticalLayoutGroup>().enabled = false;

            (_formView as RectTransform).offsetMax = new Vector2(90.0f, 0.0f);
            (_formView as RectTransform).sizeDelta = new Vector2(90.0f, extraHeight ? 20.0f : 15.0f);

            _formView.GetComponent<VerticalLayoutGroup>().enabled = true;
            _wrapper.GetComponent<VerticalLayoutGroup>().enabled = true;
        }
        #endregion

        #region UI Events
        private void OnAddToBrowserChange(bool newValue)
        {
            Plugin.Log?.Info($"CreateServerViewExtensions: OnAddToBrowserChange -> {newValue}");
            Plugin.Config.LobbyAnnounceToggle = newValue;

            // Show server browser specific settings only if toggled on
            _serverNameSetting.gameObject.SetActive(newValue);
            ReApplyVerticalLayout(newValue);
        }

        private void OnServerNameChange(string newValue)
        {
            Plugin.Log?.Info($"CreateServerViewExtensions: OnServerNameChange -> {newValue}");
            Plugin.Config.CustomGameName = newValue;

            newValue = MpSession.GetHostGameName(); // this will read CustomGameName but fall back to a default name if left empty
            Plugin.Config.CustomGameName = newValue;

            Plugin.Log.Info($"After GetHostGameName() return v3, newValue = {newValue}");

            _serverNameSetting.modalKeyboard.clearOnOpen = false;
            _serverNameSetting.modalKeyboard.keyboard.KeyboardText.text = newValue;
            _serverNameSetting.text.text = newValue;
        }
        #endregion

        #region UI Data
        public bool AddToBrowserValue
        {
            get => Plugin.Config.LobbyAnnounceToggle;
        }

        public string ServerNameValue
        {
            get => MpSession.GetHostGameName();
        }
        #endregion
    }
}
