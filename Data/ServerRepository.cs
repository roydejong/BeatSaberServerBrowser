using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerBrowser.Data.Discovery;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Data
{
    public class ServerRepository : IInitializable, ITickable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbConfig _config = null!;
        [Inject] private readonly DiContainer _container = null!;
        
        private readonly ConcurrentDictionary<string, ServerInfo> _servers = new();
        
        private bool _discoveryEnabled = false;
        private List<ServerDiscovery> _discoveryMethods = new();
        private float? _nextDiscoveryTime;
        private bool _serverListDirty;
        
        public event Action? ServersUpdatedEvent; 
        public event Action? RefreshFinishedEvent;
        
        public IReadOnlyCollection<ServerInfo> Servers => _servers.Values.ToList();

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
                    ServersUpdatedEvent?.Invoke();
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
            public bool InGameplay;
            public ConnectionMethod ConnectionMethod;
            public string? ServerCode;
            public string? ServerSecret;
            public string? RemoteAddress;
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