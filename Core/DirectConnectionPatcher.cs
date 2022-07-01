using System;
using System.Threading;
using IPA.Utilities;
using ServerBrowser.Models;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DirectConnectionPatcher : IAffinity
    {
        public BssbServer? TargetServer { get; private set; }
        public bool Enabled { get; private set; }

        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly ServerBrowserClient _bssbClient = null!;
        [Inject] private readonly LocalNetworkPlayerModel _localNetworkPlayerModel = null!;

        #region Control

        public void Enable(BssbServer server)
        {
            if (!server.IsDirectConnect)
                throw new ArgumentException("Must be direct-connect server");

            TargetServer = server;
            Enabled = true;

            _log.Info("Enable direct connect mode");
        }

        public void Disable()
        {
            TargetServer = null;
            Enabled = false;

            _log.Info("Disable direct connect mode");
        }

        #endregion

        #region Patch - Encryption

        [AffinityPrefix]
        [AffinityPatch(typeof(PacketEncryptionLayer), "MatchesFilter")]
        private bool PrefixEncryptionMatchesFilter(ref bool __result, PacketEncryptionLayer __instance)
        {
            // In direct connect mode, we will disable the unencrypted traffic filter and return true on filter checks
            // This will allow inbound unencrypted packets and prevent outbound packets from being encrypted

            __instance.filterUnencryptedTraffic = !Enabled;

            if (!Enabled)
                return true;

            __result = true;
            return false;
        }

        #endregion

        #region Patch - Connection

        [AffinityPrefix]
        [AffinityPatch(typeof(GameLiftConnectionManager), "GameLiftConnectToServer")]
        private bool PrefixGameLiftConnectToServer(string secret, string code, CancellationToken cancellationToken,
            GameLiftConnectionManager __instance)
        {
            if (!Enabled)
                return true;

            // This patch will allow us to completely bypass GameLift API calls + authentication

            __instance.InvokeMethod<object, GameLiftConnectionManager>("HandleConnectToServerSuccess", new object[]
            {
                // string playerSessionId
                "DirectConnect",
                // IPEndPoint remoteEndPoint
                TargetServer!.EndPoint!,
                // string gameSessionId,
                "DirectConnect",
                // string secret
                "DirectConnect",
                // string code
                "DirectConnect",
                // BeatmapLevelSelectionMask selectionMask
                new BeatmapLevelSelectionMask(BeatmapDifficultyMask.All, GameplayModifierMask.All,
                    SongPackMask.all),
                // GameplayServerConfiguration configuration
                new GameplayServerConfiguration(127, DiscoveryPolicy.WithCode, InvitePolicy.AnyoneCanInvite,
                    TargetServer.LogicalGameplayServerMode, TargetServer.LogicalSongSelectionMode,
                    GameplayServerControlSettings.All)
            });

            return false;
        }

        #endregion
    }
}