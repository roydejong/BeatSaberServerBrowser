using HarmonyLib;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch is helpful as it fires when MultiplayerExtensions has a settings change.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerSessionManager), "SetLocalPlayerState", MethodType.Normal)]
    class SetLocalPlayerStatePatch
    {
        static void Postfix(string state, bool hasState)
        {
            // Only update for state changes we are actually interested in
            if (state == "customsongs" || state == "modded")
            {
                Plugin.Log?.Info($"Local player state change (MultiplayerExtensions): {state}");
                GameStateManager.HandleUpdate();
            }
            else
            {
                Plugin.Log?.Debug($"Local player state change (MultiplayerExtensions): {state}");
            }
        }
    }
}
