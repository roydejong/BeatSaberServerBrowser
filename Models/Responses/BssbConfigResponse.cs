using System.Collections.Generic;
using JetBrains.Annotations;
using ServerBrowser.Data;

namespace ServerBrowser.Models.Responses
{
    [UsedImplicitly]
    public class BssbConfigResponse
    {
        public List<MasterServerRepository.MasterServerInfo> MasterServers { get; set; }
    }
}