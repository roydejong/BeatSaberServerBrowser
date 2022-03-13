using System.Collections.Generic;
using ServerBrowser.Models.Utils;

namespace ServerBrowser.Models.Requests
{
    public class AnnounceResultsData : JsonObject<AnnounceResultsData>
    {
        public string? SessionGameId;
        public List<PlayerResultsItem>? Results;

        public static AnnounceResultsData FromMultiplayerResultsData(MultiplayerResultsData data)
        {
            var p = new AnnounceResultsData();
            p.SessionGameId = data.gameId;
            p.Results = new();

            foreach (var playerResults in data.allPlayersSortedData)
            {
                p.Results.Add(PlayerResultsItem.FromMultiplayerPlayerResultsData(playerResults));
            }

            return p;
        }

        public class PlayerResultsItem
        {
            public string? UserId;
            public PlayerResultsItemBadge? Badge;
            public MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndState? LevelEndState;
            public MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason? LevelEndReason;
            public int? MultipliedScore;
            public int? ModifiedScore;
            public int? Rank;
            public int? GoodCuts;
            public int? BadCuts;
            public int? MissCount;
            public bool? FullCombo;
            public int? MaxCombo;
            
            public static PlayerResultsItem FromMultiplayerPlayerResultsData(MultiplayerPlayerResultsData data)
            {
                var item = new PlayerResultsItem();
                item.UserId = data.connectedPlayer.userId;
                
                if (data.badge is not null)
                    item.Badge = PlayerResultsItemBadge.FromMultiplayerBadgeAwardData(data.badge);

                var outerResults = data.multiplayerLevelCompletionResults;
                if (outerResults != null)
                {
                    item.LevelEndState = outerResults.playerLevelEndState;
                    item.LevelEndReason = outerResults.playerLevelEndReason;

                    var innerResults = outerResults.levelCompletionResults;
                    if (innerResults != null)
                    {
                        item.MultipliedScore = innerResults.multipliedScore;
                        item.ModifiedScore = innerResults.modifiedScore;
                        item.Rank = (int)innerResults.rank;
                        item.GoodCuts = innerResults.goodCutsCount;
                        item.BadCuts = innerResults.badCutsCount;
                        item.MissCount = innerResults.missedCount;
                        item.FullCombo = innerResults.fullCombo;
                        item.MaxCombo = innerResults.maxCombo;
                    }
                }

                return item;
            }
        }

        public class PlayerResultsItemBadge
        {
            public string? Key;
            public string? Title;
            public string? Subtitle;

            public static PlayerResultsItemBadge FromMultiplayerBadgeAwardData(MultiplayerBadgeAwardData data)
            {
                var badge = new PlayerResultsItemBadge();
                badge.Key = data.titleLocalizationKey;
                badge.Title = data.title;
                badge.Subtitle = data.subtitle;
                return badge;
            }
        }
    }
}