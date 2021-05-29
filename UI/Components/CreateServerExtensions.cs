using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.Tags.Settings;
using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.Game;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI.Components
{
    public class CreateServerExtensions : MonoBehaviour
    {
        private Transform _wrapper;
        private Transform _formView;

        private ToggleSetting _addToBrowserSetting;
        private StringSetting _serverNameSetting;

        private bool _eventsEnabled = false;
        private bool _firstEnable = true;

        public void Awake()
        {
            _wrapper = transform.Find("Wrapper");
            _formView = _wrapper.transform.Find("CreateServerFormView");

            _addToBrowserSetting = CreateToggle("Add to Server Browser", AddToBrowserValue, OnAddToBrowserChange);
            _serverNameSetting = CreateTextInput("Server Name", ServerNameValue, OnServerNameChange);

            _firstEnable = true;
        }

        public async void OnEnable()
        {
            // We're working around an issue with the BSML toggle here, it fires an incorrect change event around enable-time
            // (Also, the ~150ms delay adds a nice animation when programatically changing the user's preference so this is kinda cool)

            if (_firstEnable)
            {
                // Do this on first-enable to prevent the form view glitching (elements shifting and such)
                SyncValues();
                _firstEnable = false;
            }

            _eventsEnabled = false;

            await Task.Delay(150);

            if (this.enabled)
            {
                _eventsEnabled = true;
                SyncValues();
            }
        }

        private void SyncValues()
        {
            OnAddToBrowserChange(AddToBrowserValue);
            OnServerNameChange(ServerNameValue);
        }

        public void OnDisable()
        {
            _eventsEnabled = false;
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
            toggleSetting.toggle.onValueChanged.RemoveAllListeners();
            toggleSetting.toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                if (_eventsEnabled)
                {
                    toggleSetting.toggle.isOn = newValue;
                    onChangeCallback(newValue);
                }
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
            stringSetting.text.alignment = TextAlignmentOptions.Center;

            // Event
            stringSetting.modalKeyboard.keyboard.EnterPressed += (async delegate (string newValue)
            {
                await Task.Delay(1); // we need to run after BSML's own EnterPressed, and this, well, it works
                onChangeCallback(newValue);
            });

            // Make it look nice :-)
            var valuePicker = stringSetting.transform.Find("ValuePicker");

            var buttonLeftSide = valuePicker.Find("DecButton") as RectTransform;
            var buttonRightSide = valuePicker.Find("IncButton") as RectTransform;
            var valueText = valuePicker.Find("ValueText") as RectTransform;

            float leftSideWidth = 0.05f;

            buttonLeftSide.anchorMin = new Vector2(0.0f, 0.0f);
            buttonLeftSide.anchorMax = new Vector2(leftSideWidth, 1.0f);
            buttonLeftSide.offsetMin = new Vector2(0.0f, 0.0f);
            buttonLeftSide.offsetMax = new Vector2(0.0f, 0.0f);
            buttonLeftSide.sizeDelta = new Vector2(0.0f, 0.0f);

            buttonRightSide.anchorMin = new Vector2(leftSideWidth, 0.0f);
            buttonRightSide.anchorMax = new Vector2(1.0f, 1.0f);
            buttonRightSide.offsetMin = new Vector2(0.0f, 0.0f);
            buttonRightSide.offsetMax = new Vector2(0.0f, 0.0f);
            buttonRightSide.sizeDelta = new Vector2(0.0f, 0.0f);

            valueText.anchorMin = new Vector2(0.0f, 0.0f);
            valueText.anchorMax = new Vector2(1.0f, 1.0f);
            valueText.offsetMin = new Vector2(0.0f, -0.33f);
            valueText.offsetMax = new Vector2(0.0f, 0.0f);
            valueText.sizeDelta = new Vector2(0.0f, 0.0f);

            var editIcon = buttonRightSide.Find("EditIcon").GetComponent<ImageView>();
            editIcon.sprite = Sprites.Pencil;
            editIcon.transform.localScale = new Vector3(-1.0f, -1.0f, 1.0f);

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
            Plugin.Config.LobbyAnnounceToggle = newValue;

            // Ensure the control is in sync
            _addToBrowserSetting.toggle.isOn = newValue;

            // Show server browser specific settings only if toggled on
            _serverNameSetting.gameObject.SetActive(newValue);
            ReApplyVerticalLayout(newValue);
        }

        private void OnServerNameChange(string newValue)
        {
            Plugin.Config.CustomGameName = newValue;
            newValue = MpSession.GetHostGameName(); // this will read CustomGameName but fall back to a default name if left empty
            Plugin.Config.CustomGameName = newValue;
            _serverNameSetting.EnterPressed(newValue); // this will update both keyboard text & button face text
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
