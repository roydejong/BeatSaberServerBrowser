using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ServerBrowser.Data.Discovery;
using ServerBrowser.Models;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Data
{
    [UsedImplicitly]
    public class ServerRepository : IInitializable, ITickable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbConfig _config = null!;
        [Inject] private readonly DiContainer _container = null!;
        
        private readonly ConcurrentDictionary<string, ServerInfo> _servers = new();
        private IReadOnlyList<ServerInfo> _filteredServers = Array.Empty<ServerInfo>();
        
        public bool NoResults => _filteredServers.Count == 0;
        
        private bool _discoveryEnabled = false;
        private List<ServerDiscovery> _discoveryMethods = new();
        private float? _nextDiscoveryTime;
        private bool _serverListDirty;
        
        public event Action<IReadOnlyCollection<ServerInfo>>? ServersUpdatedEvent; 
        public event Action? RefreshFinishedEvent;

        public void Initialize()
        {
            _discoveryMethods = new List<ServerDiscovery>(3);
            _discoveryMethods.Add(new BssbApiServerDiscovery());          
            if (_config.EnableLocalNetworkDiscovery)
                _discoveryMethods.Add(new LocalNetworkServerDiscovery());
            if (_config.EnablePublicServerDiscovery)
                _discoveryMethods.Add(new PublicServerDiscovery());
            
            foreach (var discoveryMethod in _discoveryMethods)
                _container.Inject(discoveryMethod);
            
            _servers.Clear();
            _serverListDirty = true;
        }
        
        public void StartDiscovery()
        {
            if (_discoveryEnabled)
                return;
            
            _discoveryEnabled = true;
            _log.Info("Starting server discovery");
            _ = RefreshDiscovery();
        }

        private async Task RefreshDiscovery()
        {
            _nextDiscoveryTime = null;

            foreach (var discoveryMethod in _discoveryMethods)
            {
                await discoveryMethod.Refresh(this);
                
                if (_serverListDirty)
                {
                    _serverListDirty = false;
                    RaiseServersUpdated();
                }
            }

            _nextDiscoveryTime = Time.realtimeSinceStartup + DiscoveryInterval;
            RefreshFinishedEvent?.Invoke();
        }

        public void StopDiscovery()
        {
            if (!_discoveryEnabled)
                return;

            _discoveryEnabled = false;
            _log.Info("Stopping server discovery");
        }
        
        public void DiscoverServer(ServerInfo serverInfo)
        {
            _servers.AddOrUpdate(serverInfo.Key, serverInfo, (_, _) => serverInfo);
            _serverListDirty = true;
        }
        
        public void RemoveServer(string key)
        {
            _servers.TryRemove(key, out _);
            _serverListDirty = true;
        }

        public void Tick()
        {
            if (!_discoveryEnabled || !_nextDiscoveryTime.HasValue)
                return;

            if (Time.realtimeSinceStartup >= _nextDiscoveryTime)
                _ = RefreshDiscovery();
        }
        
        public string? FilterText { get; set; } = null;
        
        public void SetFilterText(string? filterText)
        {
            if (FilterText == filterText)
                return;
            
            FilterText = filterText;
            RaiseServersUpdated();
        }

        private IReadOnlyList<ServerInfo> GetFilteredServers()
        {
            var servers = _servers.Values.ToList()
                .OrderByDescending(x => x.SortPriority)
                .ToList();
            
            if (!string.IsNullOrEmpty(FilterText))
            {
                var filterChars = FilterText.Split(Array.Empty<char>(),
                    StringSplitOptions.RemoveEmptyEntries);
                var matchedResults = new List<ValueTuple<int, ServerInfo>>();
                
                foreach (var server in servers)
                {
                    var matchScore = GetTextMatchScore(server.ServerName, filterChars);
                    if (matchScore > 0)
                        matchedResults.Add((matchScore, server));
                }
                
                servers = matchedResults
                    .OrderByDescending(x => x.Item1)
                    .Select(x => x.Item2)
                    .ToList();
            }

            return servers;
        }

        private static int GetTextMatchScore(string input, string[] searchTerms)
        {
            var totalScore = 0;
            
            foreach (var searchTerm in searchTerms)
            {
                var matchIdx = input.IndexOf(searchTerm, StringComparison.CurrentCultureIgnoreCase);
                
                if (matchIdx < 0)
                    continue;
                
                totalScore += (matchIdx == 0 || char.IsWhiteSpace(input[matchIdx - 1]) ? 1 : 0) + 50 * searchTerm.Length;
            }
            
            return totalScore;
        }
        
        private void RaiseServersUpdated()
        {
            _filteredServers = GetFilteredServers();
            ServersUpdatedEvent?.Invoke(_filteredServers);
        }

        public abstract class ServerDiscovery
        {
            public abstract Task Refresh(ServerRepository repository);
        }

        public class ServerInfo
        {
            public string Key;
            public string? ImageUrl;
            public string ServerName;
            public string GameMode;
            public int PlayerCount;
            public int PlayerLimit;
            public MultiplayerLobbyState LobbyState;
            public ConnectionMethod ConnectionMethod;
            public string? ServerCode;
            public string? ServerSecret;
            
            public bool IsFull => PlayerCount >= PlayerLimit;

            public bool InGameplay =>
                LobbyState is MultiplayerLobbyState.GameRunning or MultiplayerLobbyState.GameStarting;

            public int SortPriority
            {
                get
                {
                    var sortPoints = 0;
                    if (PlayerCount >= PlayerLimit)
                        // Drop: server is full
                        sortPoints -= 10;
                    if (PlayerCount == 1)
                        // Boost: lonely player
                        sortPoints++;
                    if (PlayerLimit > 2)
                        // Boost: bigger lobby
                        sortPoints++;
                    if (!InGameplay)
                        // Boost: In lobby
                        sortPoints++;
                    return sortPoints;
                }
            }

            public string LobbyStateText
            {
                get
                {
                    return LobbyState switch
                    {
                        MultiplayerLobbyState.LobbySetup => "In lobby (setup)",
                        MultiplayerLobbyState.LobbyCountdown => "In lobby (countdown)",
                        MultiplayerLobbyState.GameStarting => "Game starting",
                        MultiplayerLobbyState.GameRunning => "Game running",
                        MultiplayerLobbyState.Error => "Error",
                        _ => "Unknown"
                    };
                
                }
            }
        }

        public enum ConnectionMethod
        {
            /// <summary>
            /// GameLift connection for official / vanilla servers.
            /// </summary>
            GameLiftOfficial = 1,
            /// <summary>
            /// Modded servers using GameLift with encryption enabled.
            /// </summary>
            GameLiftEncrypted = 2,
            /// <summary>
            /// Modded servers using GameLift with encryption disabled.
            /// </summary>
            GameLiftUnencrypted = 3,
            /// <summary>
            /// Modded servers using direct connect (no encryption).
            /// </summary>
            DirectConnect = 4
        }
        
        public const float DiscoveryInterval = 10f;
    }
}