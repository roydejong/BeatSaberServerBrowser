using BS_Utils.Utilities;
using HarmonyLib;
using HMUI;
using ServerBrowser.Core;
using UnityEngine;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// UI Patch for the native server browser content list item (GameServerListTableCell).
    /// </summary>
    [HarmonyPatch(typeof(GameServerListTableCell), "SetData", MethodType.Normal)]
    public static class GameServerListTableCellPatch
    {
        public static void Postfix(INetworkPlayer player, GameServerListTableCell __instance)
        {
            if (player is HostedGameData)
            {
                var game = (HostedGameData)player;

                // Difficulty
                var txtDifficulty = __instance.GetField<CurvedTextMeshPro>("_difficultiesText");
                txtDifficulty.SetText(game.DescribeDifficulty());

                // Type (Music Packs)
                var txtType = __instance.GetField<CurvedTextMeshPro>("_musicPackText");
                txtType.SetText(game.DescribeType());
                txtType.color = game.IsModded ? Color.white : Color.green;

                // Player Count
                var txtPlayerCount = __instance.GetField<CurvedTextMeshPro>("_playerCount");
                txtPlayerCount.color = (game.PlayerCount >= game.PlayerLimit) ? Color.gray : Color.white;
            }
        }
    }
}
