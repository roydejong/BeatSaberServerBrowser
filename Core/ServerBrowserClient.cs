using System;
using MultiplayerCore.Patchers;
using Zenject;

namespace ServerBrowser.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerBrowserClient : IInitializable, IDisposable
    {
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly IPlatformUserModel _platformUserModel = null!;
        [Inject] private readonly NetworkConfigPatcher _mpCoreNetConfig = null!;

        public UserInfo? PlatformUserInfo;

        public async void Initialize()
        {
            PlatformUserInfo = await _platformUserModel.GetUserInfo();
        }

        public void Dispose()
        {
            
        }

        public string? MasterServerHost => _mpCoreNetConfig.MasterServerEndPoint?.hostName;
        public bool UsingOfficialMaster => MasterServerHost == null || MasterServerHost.EndsWith(".beatsaber.com");
        public bool UsingBeatTogether => MasterServerHost?.EndsWith(".beattogether.systems") ?? false;

        public string PreferredServerName => (!string.IsNullOrWhiteSpace(_config.ServerName)
            ? _config.ServerName
            : DefaultServerName)!;
        public string DefaultServerName => PlatformUserInfo is not null
            ? $"{PlatformUserInfo.userName}'s game"
            : "Untitled Beat Game";
    }
}