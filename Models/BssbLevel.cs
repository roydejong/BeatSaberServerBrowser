using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models
{
    public class BssbLevel : JsonObject<BssbLevel>
    {
        public string? LevelId;
        public string? SongName;
        public string? SongSubName;
        public string? SongAuthorName;
        public string? LevelAuthorName;
    }
}