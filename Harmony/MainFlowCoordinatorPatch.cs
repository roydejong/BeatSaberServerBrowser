using HarmonyLib;
using IPA.Utilities;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch is used to provide the initial "_menuDestinationRequest" for the launch arg / steam join.
    /// </summary>
    [HarmonyPatch(typeof(MainFlowCoordinator), "DidActivate", MethodType.Normal)]
    public static class MainFlowCoordinatorPatch
    {
        public static void Prefix(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling,
            MainFlowCoordinator __instance)
        {
            if (firstActivation && GlobalModState.AutoJoinBssbKey is not null)
            {
                // We have an "auto join key" set, automatically open the multiplayer menu
                Plugin.Log?.Info("Automatically opening multiplayer sub menu (for AutoJoinBssbKey)");
                
                var mpDestination = new SelectSubMenuDestination(SelectSubMenuDestination.Destination.Multiplayer);
                __instance.SetField<MainFlowCoordinator, MenuDestination>("_menuDestinationRequest", mpDestination);    
            }
        }
    }
}