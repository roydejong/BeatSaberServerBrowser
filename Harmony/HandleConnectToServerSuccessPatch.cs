using System.Net;
using HarmonyLib;
using IPA.Utilities;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us determine the host's server secret.
    /// This can then be used to help connect to specific games without a code, e.g. in case of Quickplay games.
    /// </summary>
    [HarmonyPatch(typeof(MasterServerConnectionManager), "HandleConnectToServerSuccess")]
    public static class HandleConnectToServerSuccessPatch
    {
        public static void Postfix(string userId, string userName, IPEndPoint remoteEndPoint, string secret,
            string code, DiscoveryPolicy discoveryPolicy, InvitePolicy invitePolicy, int maxPlayerCount,
            GameplayServerConfiguration configuration, byte[] preMasterSecret, byte[] myRandom, byte[] remoteRandom,
            bool isConnectionOwner, bool isDedicatedServer)
        {
            GameStateManager.HandleConnectSuccess(code, secret, isDedicatedServer, configuration);
        }
    }
}