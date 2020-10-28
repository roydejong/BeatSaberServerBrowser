using HarmonyLib;
using LobbyBrowserMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
