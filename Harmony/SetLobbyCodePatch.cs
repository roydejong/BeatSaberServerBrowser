using HarmonyLib;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us retrieve the Server Code for the current lobby.
    /// </summary>
    [HarmonyPatch(typeof(HostLobbySetupViewController), "SetLobbyCode", MethodType.Normal)]
    public static class SetLobbyCodePatch
    {
        public static void Postfix(string code)
        {
            GameStateManager.HandleLobbyCode(code);
        }
    }
}
