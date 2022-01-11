using System;
using MultiplayerCore.Patchers;
using Zenject;

namespace ServerBrowser.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerBrowserClient : IInitializable, IDisposable
    {
        [Inject] private readonly NetworkConfigPatcher _networkConfig = null!; 
        
        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        public string? MasterServerHost => _networkConfig.MasterServerEndPoint?.hostName;
        public bool UsingOfficialMaster => MasterServerHost == null || MasterServerHost.EndsWith(".beatsaber.com");
        public bool UsingBeatTogether => MasterServerHost?.EndsWith(".beattogether.systems") ?? false;
    }
}