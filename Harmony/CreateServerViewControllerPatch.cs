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
