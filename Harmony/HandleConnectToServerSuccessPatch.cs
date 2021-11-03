using System;
using System.Net;
using HarmonyLib;
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
        public static bool Prefix(ref string userId, ref string userName, ref IPEndPoint remoteEndPoint,
            ref string secret, ref string code, ref BeatmapLevelSelectionMask selectionMask,
            ref GameplayServerConfiguration configuration, byte[] preMasterSecret,
            byte[] myRandom, byte[] remoteRandom, bool isConnectionOwner, bool isDedicatedServer, string managerId,
            MasterServerConnectionManager __instance)
        {
            Plugin.Log.Info($"HandleConnectToServerSuccess (userId={userId}, userName={userName}," +
                            $" remoteEndPoint={remoteEndPoint}, secret={secret}, code={code}," +
                            $" isDedicatedServer={isDedicatedServer}, managerId={managerId})");
            
            if (GlobalModState.WeInitiatedConnection && GlobalModState.LastConnectToHostedGame != null)
            {
                // Server Browser initiated this connection attempt
                if (GlobalModState.DirectConnectTarget != null)
                {
                    // Direct connection attempt, ignore the master server target
                    Plugin.Log.Warn("HandleConnectToServerSuccess: Forcing direct connection override" +
                                    $" (DirectConnectTarget={GlobalModState.DirectConnectTarget})");

                    var targetGame = GlobalModState.LastConnectToHostedGame;
                    
                    userId = targetGame.OwnerId;
                    userName = targetGame.OwnerName;
                    remoteEndPoint = GlobalModState.DirectConnectTarget;
                    secret = targetGame.HostSecret ?? "";
                    code = targetGame.ServerCode;
                    selectionMask = new BeatmapLevelSelectionMask(BeatmapDifficultyMask.All, 
                        GameplayModifierMask.All, SongPackMask.all);
                    managerId = targetGame.ManagerId ?? targetGame.OwnerId;

                    if (targetGame.IsQuickPlayServer)
                    {
                        configuration = new GameplayServerConfiguration(targetGame.PlayerLimit,
                            DiscoveryPolicy.Public, InvitePolicy.AnyoneCanInvite, GameplayServerMode.Countdown,
                            SongSelectionMode.Vote, GameplayServerControlSettings.All);
                    }
                    else
                    {
                        configuration = new GameplayServerConfiguration(targetGame.PlayerLimit,
                            DiscoveryPolicy.Public, InvitePolicy.AnyoneCanInvite, GameplayServerMode.Managed,
                            SongSelectionMode.OwnerPicks, GameplayServerControlSettings.All);
                    }
                    
                    GlobalModState.ShouldDisableEncryption = true; // about to talk to game server, disable encryption
                }
                else
                {
                    // Normal connection attempt, verify the master server target
                    var targetGame = GlobalModState.LastConnectToHostedGame;
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
                        GlobalModState.WeAbortedJoin = true;
                        MpModeSelection.CancelLobbyJoin(hideLoading: false);
                        return false;
                    }
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