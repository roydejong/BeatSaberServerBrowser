using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;
using LiteNetLib.Utils;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Network.Discovery
{
    [UsedImplicitly]
    public class DiscoveryClient : MonoBehaviour, IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;
        
        private UdpClient? _udpClient;
        private float? _lastBroadcastTime;
        
        private readonly NetDataReader _netDataReader = new();
        private readonly NetDataWriter _netDataWriter = new(true, 256);

        public readonly Queue<DiscoveryResponsePacket> ReceivedResponses = new();
        
        public bool IsActive => _udpClient != null;

        public void StartBroadcast()
        {
            if (_udpClient != null)
                StopBroadcast();
            
            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            _udpClient.EnableBroadcast = true;

            _lastBroadcastTime = null;
            
            ReceivedResponses.Clear();
            
            _log.Debug("Started local network discovery");
        }

        public void StopBroadcast()
        {
            if (_udpClient == null)
                return;
            
            _udpClient?.Dispose();
            _udpClient = null;
            
            ReceivedResponses.Clear();
            
            _log.Debug("Stopped local network discovery");
        }

        public void OnDisable()
        {
            StopBroadcast();
        }

        public void Dispose()
        {
            _udpClient?.Dispose();
            _udpClient = null;
            
            ReceivedResponses.Clear();
        }

        public void LateUpdate()
        {
            if (_udpClient == null)
                return;

            if (_lastBroadcastTime == null || (Time.time - _lastBroadcastTime.Value) >= DiscoveryInterval)
                SendBroadcast();

            if (_udpClient.Available > 0)
                HandleReceive();
        }

        private void HandleReceive()
        {
            var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            var data = _udpClient!.Receive(ref remoteEndpoint);
            
            _netDataReader.SetSource(data);

            while (_netDataReader.AvailableBytes > 0)
            {
                try
                {
                    var packet = new DiscoveryResponsePacket();
                    packet.Deserialize(_netDataReader);
                    HandleResponse(packet);
                }
                catch (Exception e)
                {
                    _log.Warn($"Failed to deserialize local discovery response: {e}");
                }
            }
        }
        
        private void HandleResponse(DiscoveryResponsePacket packet)
        {
            if (packet.Prefix != DiscoveryConsts.PacketPrefix)
                return;
            
            ReceivedResponses.Enqueue(packet);
        }

        private void SendBroadcast()
        {
            _netDataWriter.Reset();
            QueryPacket.Serialize(_netDataWriter);
            
            _udpClient!.Send(_netDataWriter.Data, _netDataWriter.Length, BroadcastEndpoint);
            _lastBroadcastTime = Time.time;
        }
        
        public const float DiscoveryInterval = 5f;

        public static readonly IPEndPoint BroadcastEndpoint = new(IPAddress.Broadcast, DiscoveryConsts.BroadcastPort);

        public static readonly DiscoveryQueryPacket QueryPacket = new()
        {
            Prefix = DiscoveryConsts.PacketPrefix,
            ProtocolVersion = DiscoveryConsts.ProtocolVersion
        };
    }
}