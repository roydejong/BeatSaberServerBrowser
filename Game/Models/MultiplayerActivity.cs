using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ServerBrowser.Core;
using ServerBrowser.Utils;
using static MultiplayerLobbyConnectionController;

namespace ServerBrowser.Game.Models
{
    /// <summary>
    /// The executive summary of the current local multiplayer state.
    /// </summary>
    public sealed class MultiplayerActivity
    {
        #region Fields
        public string Name;
        public MasterServerEndPoint MasterServer;
        public LobbyConnectionType ConnectionType;
        public MultiplayerLobbyState LobbyState;
        public string? ServerCode;
        public string? HostUserId;
        public string? HostSecret;
        public bool IsDedicatedServer;
        public GameplayServerConfiguration? ServerConfiguration;
        public int MaxPlayerCount;
        public List<IConnectedPlayer>? Players;
        public IPreviewBeatmapLevel? CurrentLevel;
        public BeatmapDifficulty? CurrentDifficulty;
        public BeatmapCharacteristicSO? CurrentCharacteristic;
        public GameplayModifiers? CurrentModifiers;
        #endregion

        #region Getters
        public int CurrentPlayerCount => Players?.Count ?? 1;
        
        public bool IsInMultiplayer => ConnectionType != LobbyConnectionType.None &&
                                       LobbyState != MultiplayerLobbyState.None &&
                                       LobbyState != MultiplayerLobbyState.Error;

        public bool IsInGameplay => LobbyState == MultiplayerLobbyState.GameRunning;

        public bool IsHost => ConnectionType == LobbyConnectionType.PartyHost;

        public bool IsQuickPlay => ConnectionType == LobbyConnectionType.QuickPlay;

        public string CurrentDifficultyName => CurrentDifficulty.ToStringWithSpaces();

        public string DifficultyMaskName => ServerConfiguration.HasValue
            ? ServerConfiguration.Value.difficulties.ToStringWithSpaces()
            : "All";

        public IConnectedPlayer? ConnectionOwner => Players?.FirstOrDefault(p => p.isConnectionOwner);
        
        public bool IsBeatDedi => ConnectionOwner?.userName.StartsWith("BeatDedi/") ?? false;

        public bool IsModded => ConnectionOwner.HasState("modded") || ConnectionOwner.HasState("customsongs");
        #endregion

        #region Announce helpers
        public string DetermineServerType()
        {
            if (IsQuickPlay)
                if (IsBeatDedi)
                    return HostedGameData.ServerTypeBeatDediQuickplay;
                else
                    return HostedGameData.ServerTypeVanillaQuickplay;
            else
                if (IsBeatDedi)
                    return HostedGameData.ServerTypeBeatDediCustom;
                else
                    return HostedGameData.ServerTypePlayerHost;
        }

        public IEnumerable<HostedGamePlayer> GetPlayersForAnnounce()
        {
            if (Players != null)
            {
                foreach (var player in Players)
                {
                    yield return new HostedGamePlayer()
                    {
                        SortIndex = player.sortIndex,
                        UserId = player.userId,
                        UserName = player.userName,
                        IsHost = player.isConnectionOwner,
                        Latency = player.currentLatency
                    };
                }
            }
        }

        public BeatmapDifficulty? DetermineLobbyDifficulty()
        {
            var difficulty = CurrentDifficulty;
            if (difficulty == null && ServerConfiguration.HasValue)
                difficulty = ServerConfiguration.Value.difficulties.FromMask();
            return difficulty;
        }
        #endregion
    }
}