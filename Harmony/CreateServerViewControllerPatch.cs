using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Tags;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(CreateServerViewController), "DidActivate", MethodType.Normal)]
    public static class CreateServerViewControllerDidActivatePatch
    {
        public static void Postfix(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling, CreateServerViewController __instance)
        {
            var wrapper = __instance.transform.Find("Wrapper");
            var formView = wrapper.transform.Find("CreateServerFormView");

            if (firstActivation)
            {
                (formView as RectTransform).offsetMax = new Vector2(90.0f, 0.0f);
                wrapper.GetComponent<VerticalLayoutGroup>().enabled = true;
                formView.GetComponent<VerticalLayoutGroup>().enabled = true;

                var toggleTag = new ToggleSettingTag();
                var toggleTagObj = toggleTag.CreateObject(formView);
                (toggleTagObj.transform as RectTransform).sizeDelta = new Vector2(90.0f, 7.0f);

                var toggleSetting = toggleTagObj.GetComponent<ToggleSetting>();
                toggleSetting.text.SetText("Add to Server Browser");
                toggleSetting.toggle.isOn = Plugin.Config.LobbyAnnounceToggle;

                toggleSetting.toggle.onValueChanged.AddListener(delegate (bool value)
                {
                    Plugin.Config.LobbyAnnounceToggle = value;
                });
            }
        }
    }

    [HarmonyPatch(typeof(CreateServerViewController), "CreatePartyConfig", MethodType.Normal)]
    public static class CreateServerViewControllerCreatePartyConfigPatch
    {
        public static void Postfix(CreateServerViewController __instance, ref UnifiedNetworkPlayerModel.CreatePartyConfig __result)
        {
            if (Plugin.Config.LobbyAnnounceToggle)
            {
                __result.discoveryPolicy = DiscoveryPolicy.Public;
                __result.invitePolicy = InvitePolicy.AnyoneCanInvite;
            }
        }
    }
}
