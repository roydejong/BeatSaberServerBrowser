using HarmonyLib;
using IPA.Utilities;
using ServerBrowser.Game;

// ReSharper disable InconsistentNaming

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch triggers the internal LobbyStateChanged event.
    /// </summary>
    [HarmonyPatch(typeof(LobbyGameStateController), "state", MethodType.Setter)]
    public static class MpLobbyStatePatch
    {
        public static void Postfix(LobbyGameStateController __instance)
        {
            var nextState = __instance.GetProperty<MultiplayerLobbyState, LobbyGameStateController>("state");
            MpEvents.RaiseLobbyStateChanged(__instance, nextState);
        }
    }
}