using System.Net;
using HarmonyLib;
using IPA.Utilities;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.Game.Models;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us determine the host's server secret.
    /// This can then be used to help connect to specific games without a code, e.g. in case of Quickplay games.
    /// </summary>
    [HarmonyPatch(typeof(MasterServerConnectionManager), "HandleConnectToServerSuccess")]
    public static class HandleConnectToServerSuccessPatch
    {
        public static bool Prefix(string userId, string userName, IPEndPoint remoteEndPoint, string secret,
            string code, DiscoveryPolicy discoveryPolicy, InvitePolicy invitePolicy, int maxPlayerCount,
            GameplayServerConfiguration configuration, byte[] preMasterSecret, byte[] myRandom, byte[] remoteRandom,
            bool isConnectionOwner, bool isDedicatedServer, MasterServerConnectionManager __instance)
        {
            if (isDedicatedServer && MpModeSelection.WeInitiatedConnection &&
                (MpModeSelection.InjectQuickPlaySecret != secret || MpModeSelection.InjectServerCode != code))
            {
                // Matchmaking put us in the wrong Quick Play lobby, which means our injected secret failed
                Plugin.Log?.Warn($"HandleConnectToServerSuccess: Matchmaking did not put us in the expected " +
                                 $"Quick Play lobby (InjectQuickPlaySecret={MpModeSelection.InjectQuickPlaySecret}, " +
                                 $"ActualSecret={secret}, InjectServerCode={MpModeSelection.InjectServerCode}, " +
                                 $"ActualServerCode={code})");
                MpModeSelection.CancelLobbyJoin(hideLoading: false);
                return false;
            }

            // Track the server connection and update game state as needed 
            MpEvents.RaiseBeforeConnectToServer(__instance, new ConnectToServerEventArgs()
            {
                UserId = userId,
                UserName = userName,
                RemoteEndPoint = remoteEndPoint,
                Secret = secret,
                Code = code,
                DiscoveryPolicy = discoveryPolicy,
                InvitePolicy = invitePolicy,
                MaxPlayerCount = maxPlayerCount,
                Configuration = configuration,
                IsDedicatedServer = isDedicatedServer,
                IsConnectionOwner = isConnectionOwner
            });
            
            return true;
        }
    }
}