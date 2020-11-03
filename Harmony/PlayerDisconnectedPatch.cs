﻿using HarmonyLib;
using ServerBrowser;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    class PlayerDisconnectedPatch
    {
        /// <summary>
        /// This patch helps us send an update to the master server if the player count changes.
        /// </summary>
        [HarmonyPatch(typeof(MultiplayerSessionManager), "HandlePlayerDisconnected", MethodType.Normal)]
        static void Postfix(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"Player disconnected: {player.userId}, {player.userName}");
            GameStateManager.HandleUpdate();
        }
    }
}