using System.Threading.Tasks;

namespace ServerBrowser.Data.Discovery
{
    public class PublicServerDiscovery : ServerRepository.ServerDiscovery
    {
        public override async Task Refresh(ServerRepository repository)
        {
            // TODO: Check if this works: official server listing from GameLift. I have no idea
        }
    }
}