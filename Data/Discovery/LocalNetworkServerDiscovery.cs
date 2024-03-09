using System.Threading.Tasks;

namespace ServerBrowser.Data.Discovery
{
    public class LocalNetworkServerDiscovery : ServerRepository.ServerDiscovery
    {
        public override async Task Refresh(ServerRepository repository)
        {
            // TODO: Figure out / publish a standard for local network discovery
        }
    }
}