using HarmonyLib;
using LobbyBrowserMod.Core;

namespace LobbyBrowserMod.Harmony
{
    [HarmonyPatch(typeof(MultiplayerSessionManager), "SetLocalPlayerState", MethodType.Normal)]
    class SetLocalPlayerStatePatch
    {
        static void Postfix(string state, bool hasState)
        {
            // This patch is helpful as it fires when MultiplayerExtensions has a settings change
            Plugin.Log?.Info($"Local player state change: {state}");
            LobbyStateManager.HandleUpdate();
        }
    }
}
