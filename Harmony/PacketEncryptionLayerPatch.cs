using System;
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
                
                encrypted = false;
                
                if (offset == 0)
                {
                    Array.Copy(data, offset, data, offset + 1, length);
                }
                else
                {
                    offset--;
                }
                
                length++;
                data[offset] = 0;
                
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