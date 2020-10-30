using HarmonyLib;
using IPA.Utilities;
using LobbyBrowserMod.Core;

namespace LobbyBrowserMod.Harmony
{
    /// <summary>
    /// This patch lets us query information about the multiplayer lobby.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLobbyConnectionController), "connectionType", MethodType.Setter)]
    class LobbyConnectionTypePatch
    {
        public static MultiplayerLobbyConnectionController.LobbyConnectionType ConnectionType { get; private set; }
            = MultiplayerLobbyConnectionController.LobbyConnectionType.None;

        public static void Prefix(MultiplayerLobbyConnectionController __instance)
        {
            ConnectionType = __instance.GetProperty<MultiplayerLobbyConnectionController.LobbyConnectionType, MultiplayerLobbyConnectionController>("connectionType");

            Plugin.Log?.Info($"Lobby state change: {ConnectionType} (IsPartyMultiplayer: {IsPartyMultiplayer}, IsPartyHost: {IsPartyHost})");

            LobbyStateManager.HandleUpdate();
        }

        public static bool IsPartyHost
        {
            get => ConnectionType == MultiplayerLobbyConnectionController.LobbyConnectionType.PartyHost;
        }

        public static bool IsPartyMultiplayer
        {
            get => ConnectionType == MultiplayerLobbyConnectionController.LobbyConnectionType.PartyHost ||
                ConnectionType == MultiplayerLobbyConnectionController.LobbyConnectionType.PartyClient;
        }
    }
}
