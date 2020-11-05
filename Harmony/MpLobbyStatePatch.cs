using HarmonyLib;
using IPA.Utilities;
using ServerBrowser.Core;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us determine lobby state changes (from lobby to in-game, and vice versa).
    /// </summary>
    [HarmonyPatch(typeof(LobbyGameStateController), "state", MethodType.Setter)]
    class MpLobbyStatePatch
    {
        public static MultiplayerLobbyState LobbyState { get; private set; } = MultiplayerLobbyState.None;

        public static void Postfix(LobbyGameStateController __instance)
        {
            var nextState = __instance.GetProperty<MultiplayerLobbyState, LobbyGameStateController>("state");

            if (nextState != LobbyState)
            {
                LobbyState = nextState;
                Plugin.Log?.Info($"Lobby state change: {LobbyState}");

                GameStateManager.HandleUpdate();
            }
        }

        public static bool IsValidMpState
        {
            get => LobbyState == MultiplayerLobbyState.LobbySetup
                || LobbyState == MultiplayerLobbyState.GameStarting
                || LobbyState == MultiplayerLobbyState.GameRunning;
        }

        public static bool IsInGame
        {
            get => LobbyState == MultiplayerLobbyState.GameRunning;
        }
    }
}
