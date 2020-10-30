using HarmonyLib;
using LobbyBrowserMod.Core;

namespace LobbyBrowserMod.Harmony
{
    /// <summary>
    /// This patch lets us retrieve the Server Code for the current lobby.
    /// </summary>
    [HarmonyPatch(typeof(HostLobbySetupViewController), "SetLobbyCode", MethodType.Normal)]
    class LobbyCodePatch
    {
        static void Postfix(string code)
        {
            LobbyStateManager.HandleLobbyCode(code);
        }
    }
}
