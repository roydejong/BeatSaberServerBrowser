using BeatSaberMarkupLanguage.Attributes;
using Newtonsoft.Json;
using ServerBrowser.Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBrowser.Core
{
    public class HostedGameData
    {
        public int? Id { get; set; }
        public string ServerCode { get; set; }
        public string GameName { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int PlayerCount { get; set; }
        public int PlayerLimit { get; set; }
        public bool IsModded { get; set; }
        public MultiplayerLobbyState LobbyState { get; set; } = MultiplayerLobbyState.None;
        public string LevelId { get; set; } = null;
        public string SongName { get; set; } = null;
        public string SongAuthor { get; set; } = null;
        public BeatmapDifficulty? Difficulty { get; set; }
        public string Platform { get; set; } = Plugin.PLATFORM_UNKNOWN;

        public string Describe()
        {
            var moddedDescr = IsModded ? "Modded" : "Vanilla";
            var stateDescr = MpLobbyStatePatch.IsInGame ? "In game" : "In lobby";

            return $"{GameName} ({PlayerCount}/{PlayerLimit} players, {stateDescr}, {moddedDescr})";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
