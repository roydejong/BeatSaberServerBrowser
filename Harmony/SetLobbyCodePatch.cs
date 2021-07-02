using HarmonyLib;
using ServerBrowser.Core;
using ServerBrowser.Game;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us retrieve the Server Code for the current lobby.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerSettingsPanelController), "SetLobbyCode", MethodType.Normal)]
    public static class SetLobbyCodePatch
    {
        private static string? _lastCode = null;
        
        public static void Postfix(string code, MultiplayerSettingsPanelController __instance)
        {
            if (_lastCode != code)
            {
                _lastCode = code;
                MpEvents.RaiseServerCodeChanged(__instance, code);
            }
        }
    }
}
