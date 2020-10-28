using HarmonyLib;
using LobbyBrowserMod.Core;

namespace LobbyBrowserMod.Harmony
{
    [HarmonyPatch(typeof(HostLobbySetupViewController), "SetLobbyCode", MethodType.Normal)]
    class LobbyCodePatch
    {
        static void Postfix(string code)
        {
            LobbyStateManager.HandleLobbyCode(code);
        }
    }
}
