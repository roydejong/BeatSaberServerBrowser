using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using ServerBrowser.Models;
using ServerBrowser.UI.Components;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Views
{
    [HotReload]
    public class ServerBrowserDetailViewController : BSMLAutomaticViewController
    {
        [Inject] protected readonly DiContainer _container = null!;
        
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [UIComponent("mainContentRoot")] private VerticalLayoutGroup _mainContentRoot = null!;
        [UIComponent("titleBarRoot")] private VerticalLayoutGroup _titleBarRoot = null!;
        [UIComponent("playerList-scroll")] private BSMLScrollableContainer _playerListScrollable = null!;
        [UIComponent("playerListRoot")] private VerticalLayoutGroup _playerListRoot = null!;
        [UIComponent("playerListRowPrefab")] private HorizontalLayoutGroup _playerListRowPrefab = null!;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        private BssbLevelBarClone _levelBar = null!;
        private BssbPlayersTable _playersTable = null!;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            // Create BssbLevelBarClone for title
            _levelBar = BssbLevelBarClone.Create(_container, _titleBarRoot.transform);
            _levelBar.SetImageVisible(true);
            _levelBar.SetBackgroundStyle(BssbLevelBarClone.BackgroundStyle.Test);
            _levelBar.SetText("!Server name!", "!Server type!");
            
            // Create players table with prefab
            _playerListRowPrefab.gameObject.SetActive(false);
            _playersTable = new BssbPlayersTable(_playerListRoot.gameObject, _playerListRowPrefab.gameObject);
            
            SetPlayerData();
        }

        public void SetData(object serverInfo)
        {
            // TODO
            
            SetPlayerData();
        }

        private void SetPlayerData()
        {
            var testData = new List<BssbServerPlayer>();
            
            testData.Add(new BssbServerPlayer() { UserId = $"dummy_user_host", UserName = $"The Server", IsHost = true });
            testData.Add(new BssbServerPlayer() { UserId = $"dummy_user_leader", UserName = $"The Leader", IsPartyLeader = true });
            testData.Add(new BssbServerPlayer() { UserId = $"dummy_user_announcer", UserName = $"The Announcer", IsAnnouncing = true });
            
            for (var i = 0; i < 5; i++)
                testData.Add(new BssbServerPlayer() { UserId = $"dummy_user_{i}", UserName = $"User {i}" });
            
            _playersTable.SetData(testData);
            
            _playerListScrollable.ContentSizeUpdated();
            _playerListScrollable.ScrollTo(0, false);
        }
    }
}