using HarmonyLib;
using ServerBrowser.Game;
using System;

namespace ServerBrowser.Core
{
    [HarmonyPatch(typeof(NetworkConfigSO))]
    public class MasterServerEndPointPatch
    {
        [HarmonyPatch(MethodType.Getter)]
        [HarmonyPatch("masterServerEndPoint")]
        [HarmonyPatch(new Type[] { typeof(MasterServerEndPoint) })]
        [HarmonyPriority(Priority.Last)] // it's important we take priority over any mod plugins
        public static void Postfix(NetworkConfigSO __instance, ref MasterServerEndPoint __result)
        {
            MpConnect.ReportCurrentMasterServerValue(__result);

            if (MpConnect.OverrideEndPoint != null)
            {
                // We are overriding the endpoint, to replace either the official or modded value
                __result = MpConnect.OverrideEndPoint;
            }
        }
    }
}
