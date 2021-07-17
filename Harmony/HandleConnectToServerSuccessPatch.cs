using System;
using System.Net;
using HarmonyLib;
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
        public static bool Prefix(string userId, string userName, IPEndPoint remoteEndPoint, string secret, string code,
            BeatmapLevelSelectionMask selectionMask, GameplayServerConfiguration configuration, byte[] preMasterSecret,
            byte[] myRandom, byte[] remoteRandom, bool isConnectionOwner, bool isDedicatedServer, string managerId,
            MasterServerConnectionManager __instance)
        {
            // If we initiated the connection, make sure the master server didn't put us in the wrong lobby
            if (MpModeSelection.WeInitiatedConnection && MpModeSelection.LastConnectToHostedGame != null)
            {
                var targetGame = MpModeSelection.LastConnectToHostedGame;
                var isValidJoin = true;

                if (!String.IsNullOrEmpty(targetGame.ServerCode) && code != targetGame.ServerCode)
                {
                    // Server code mismatch
                    Plugin.Log.Warn("HandleConnectToServerSuccess: Server Code mismatch" +
                                    $" (Expected={targetGame.ServerCode}, Actual={code})");
                    isValidJoin = false;
                }
                
                if (!String.IsNullOrEmpty(targetGame.HostSecret) && !String.IsNullOrEmpty(secret)
                                                                 && secret != targetGame.HostSecret)
                {
                    // Host secret mismatch
                    Plugin.Log.Warn("HandleConnectToServerSuccess: Host Secret mismatch" +
                                    $" (Expected={targetGame.HostSecret}, Actual={secret})");
                    isValidJoin = false;
                }

                if (!isValidJoin)
                {
                    MpModeSelection.WeAbortedJoin = true;
                    MpModeSelection.CancelLobbyJoin(hideLoading: false);
                    return false;
                }
            }
            
            // Track the server connection and update game state as needed 
            MpEvents.RaiseBeforeConnectToServer(__instance, new ConnectToServerEventArgs()
            {
                UserId = userId,
                UserName = userName,
                RemoteEndPoint = remoteEndPoint,
                Secret = secret,
                Code = code,
                SelectionMask = selectionMask,
                Configuration = configuration,
                IsDedicatedServer = isDedicatedServer,
                IsConnectionOwner = isConnectionOwner,
                ManagerId = managerId
            });
            
            return true;
        }
    }
}