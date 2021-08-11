using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using ServerBrowser.Game;
using ServerBrowser.Utils.Serialization;
using Version = Hive.Versioning.Version;

namespace ServerBrowser.Core
{
    public class HostedGameData
    {
        #region Consts
        public const string ServerTypePlayerHost = "player_host";
        public const string ServerTypeBeatDediCustom = "beatdedi_custom";
        public const string ServerTypeBeatDediQuickplay = "beatdedi_quickplay";
        public const string ServerTypeVanillaQuickplay = "vanilla_quickplay";
        #endregion
        
        #region Fields
        public int? Id { get; set; }
        public string ServerCode { get; set; }
        public string GameName { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int PlayerCount { get; set; }
        public int PlayerLimit { get; set; }
        public bool IsModded { get; set; }
        public MultiplayerLobbyState LobbyState { get; set; } = MultiplayerLobbyState.None;
        public string? LevelId { get; set; } = null;
        public string? SongName { get; set; } = null;
        public string? SongAuthor { get; set; } = null;
        public BeatmapDifficulty? Difficulty { get; set; }
        public string Platform { get; set; } = "unknown";
        public string? MasterServerHost { get; set; } = null;
        public int? MasterServerPort { get; set; } = null;
        public string? CoverUrl { get; set; } = null;
        public List<HostedGamePlayer>? Players { get; set; } = null;
        [JsonConverter(typeof(HiveVersionJsonConverter))]
        public Version? MpExVersion { get; set; } = null;
        public string? ServerType { get; set; } = null;
        public string? HostSecret { get; set; } = null;
        [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint? Endpoint { get; set; } = null;
        #endregion

        #region Helpers
        [JsonIgnoreAttribute]
        public bool IsOnCustomMaster => !String.IsNullOrEmpty(MasterServerHost) && !MasterServerHost.EndsWith(MpConnect.OFFICIAL_MASTER_SUFFIX);

        [JsonIgnoreAttribute]
        public bool IsDedicatedServer => ServerType == ServerTypeBeatDediCustom || ServerType == ServerTypeBeatDediQuickplay ||
                                         ServerType == ServerTypeVanillaQuickplay;

        [JsonIgnoreAttribute]
        public bool IsBeatDedi => ServerType == ServerTypeBeatDediCustom || ServerType == ServerTypeBeatDediQuickplay;

        [JsonIgnoreAttribute]
        public bool SupportsDirectConnect => IsBeatDedi;

        [JsonIgnoreAttribute]
        public bool IsQuickPlayServer => ServerType == ServerTypeBeatDediQuickplay || ServerType == ServerTypeVanillaQuickplay;
        
        [JsonIgnoreAttribute]
        public bool CanJoin => !String.IsNullOrEmpty(ServerCode) || !String.IsNullOrEmpty(HostSecret);
        #endregion

        #region Describe
        public string Describe()
        {
            var moddedDescr = IsModded ? "Modded" : "Vanilla";

            if (IsQuickPlayServer)
            {
                moddedDescr += " Quick Play";
            }
            
            if (IsOnCustomMaster)
            {
                moddedDescr += ", Cross-play";
            }

            return $"{GameName} ({PlayerCount}/{PlayerLimit}, {moddedDescr})";
        }

        public string DescribeType()
        {
            string masterServerDescr;
            string moddedDescr;

            if (String.IsNullOrEmpty(MasterServerHost) || MasterServerHost == MpConnect.OFFICIAL_MASTER_STEAM)
            {
                masterServerDescr = "Steam";
            }
            else if (MasterServerHost == MpConnect.OFFICIAL_MASTER_OCULUS)
            {
                masterServerDescr = "Oculus";
            }
            else if (MasterServerHost.EndsWith(MpConnect.OFFICIAL_MASTER_SUFFIX))
            {
                masterServerDescr = "Official-unknown";
            }
            else
            {
                masterServerDescr = "Cross-play";
            }

            if (IsModded)
            {
                moddedDescr = "Modded";

                if (MpExVersion != null)
                {
                    moddedDescr += $" {MpExVersion}";
                }
            }
            else
            {
                moddedDescr = "Vanilla";
            }

            return $"{masterServerDescr}, {moddedDescr}";
        }

        public string DescribeDifficulty(bool withColorTag = false)
        {
            string text;

            switch (Difficulty)
            {
                default:
                    text = Difficulty.ToString();
                    break;
                case BeatmapDifficulty.ExpertPlus:
                    text = "Expert+";
                    break;
            }

            if (withColorTag)
            {
                string colorHex;

                switch (Difficulty)
                {
                    default:
                    case BeatmapDifficulty.Easy:
                        colorHex = "#3cb371";
                        break;
                    case BeatmapDifficulty.Normal:
                        colorHex = "#59b0f4";
                        break;
                    case BeatmapDifficulty.Hard:
                        colorHex = "#ff6347";
                        break;
                    case BeatmapDifficulty.Expert:
                        colorHex = "#bf2a42";
                        break;
                    case BeatmapDifficulty.ExpertPlus:
                        colorHex = "#8f48db";
                        break;
                }

                text = $"<color={colorHex}>{text}</color>";
            }

            return text;
        }
        #endregion

        #region Serialize
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
