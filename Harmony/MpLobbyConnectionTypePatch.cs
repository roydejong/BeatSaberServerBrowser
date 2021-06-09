using HarmonyLib;
using IPA.Utilities;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us query information about the multiplayer lobby.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLobbyConnectionController), "connectionType", MethodType.Setter)]
    public static class MpLobbyConnectionTypePatch
    {
        public static MultiplayerLobbyConnectionController.LobbyConnectionType ConnectionType { get; private set; }
            = MultiplayerLobbyConnectionController.LobbyConnectionType.None;

        public static void Prefix(MultiplayerLobbyConnectionController __instance)
        {
            var nextConnectionType = __instance.GetProperty<MultiplayerLobbyConnectionController.LobbyConnectionType, MultiplayerLobbyConnectionController>("connectionType");

            if (nextConnectionType != ConnectionType)
            {
                ConnectionType = nextConnectionType;
                Plugin.Log?.Debug($"Lobby connection type change: {ConnectionType} (IsPartyMultiplayer: {IsPartyMultiplayer}, IsPartyHost: {IsPartyHost})");
                GameStateManager.HandleUpdate();
            }
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

        public static bool IsQuickplay
        {
            get => ConnectionType == MultiplayerLobbyConnectionController.LobbyConnectionType.QuickPlay;
        }
    }
}
