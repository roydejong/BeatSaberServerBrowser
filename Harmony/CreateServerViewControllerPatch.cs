using HarmonyLib;
using ServerBrowser.UI.Components;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(CreateServerViewController), "DidActivate", MethodType.Normal)]
    public static class CreateServerViewControllerDidActivatePatch
    {
        public static void Postfix(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling, CreateServerViewController __instance)
        {
            if (firstActivation)
            {
                __instance.gameObject.AddComponent<CreateServerExtensions>();
            }
        }
    }

    [HarmonyPatch(typeof(CreateServerViewController), "ApplyAndGetData", MethodType.Normal)]
    public static class CreateServerFormDataPatch
    {
        public static void Postfix(ref CreateServerFormData __result)
        {
            if (Plugin.Config.LobbyAnnounceToggle)
            {
                __result.netDiscoverable = true;
                __result.allowInviteOthers = true;
                __result.usePassword = false;
            }
        }
    }
}
