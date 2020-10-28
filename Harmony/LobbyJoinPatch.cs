using LobbyBrowserMod.Ui;
using HarmonyLib;
using IPA.Utilities;

namespace LobbyBrowserMod.Harmony
{
    [HarmonyPatch(typeof(MultiplayerLobbyConnectionController), "connectionType", MethodType.Setter)]
    class LobbyJoinPatch
    {
        public static MultiplayerLobbyConnectionController.LobbyConnectionType ConnectionType
        {
            get;
            private set;
        }

        public static void Prefix(MultiplayerLobbyConnectionController __instance)
        {
            ConnectionType = __instance.GetProperty<MultiplayerLobbyConnectionController.LobbyConnectionType, MultiplayerLobbyConnectionController>("connectionType");

            Plugin.Log.Info($"Lobby state change: {ConnectionType} (IsPartyMultiplayer: {IsPartyMultiplayer}, IsPartyHost: {IsPartyHost})");

            LobbyConfigPanel.instance.UpdateState();
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
