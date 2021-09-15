using HarmonyLib;
using ServerBrowser.Game;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us detect host transitions and party leader assignments.
    /// </summary>
    [HarmonyPatch(typeof(LobbyPlayersDataModel), "SetPlayerIsPartyOwner", MethodType.Normal)]
    public static class SetPlayerIsPartyOwnerPatch
    {
        public static void Postfix(string userId, bool isPartyOwner, bool notifyChange,
            LobbyPlayersDataModel __instance)
        {
            if (isPartyOwner)
            {
                MpEvents.RaisePartyOwnerChanged(__instance, userId);
            }
        }
    }
}