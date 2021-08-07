using System.Net;
using HarmonyLib;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    [HarmonyPatch(typeof(PacketEncryptionLayer), "ProcessOutBoundPacketInternal", MethodType.Normal)]
    public static class PacketEncryptionLayerPatch
    {
        public static bool Prefix(IPEndPoint remoteEndPoint, ref byte[] data, ref int offset, ref int length,
            out bool encrypted, ref bool __result, PacketEncryptionLayer __instance)
        {
            if (GlobalModState.ShouldDisableEncryption)
            {
                // Disabling; do not perform outbound encryption, disable unencrypted traffic filter
                if (__instance.filterUnencryptedTraffic)
                    __instance.filterUnencryptedTraffic = false;
                
                Plugin.Log.Warn("ProcessOutBoundPacketInternal: Hit disable patch");

                encrypted = false;
                __result = true;
                return false;
            }
            
            // Not disabling; run regular code, re-enable the unencrypted traffic filter (regular game behavior)
            if (!__instance.filterUnencryptedTraffic)
                __instance.filterUnencryptedTraffic = true;

            encrypted = false;
            return true;
        }
    }
}