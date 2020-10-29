using LobbyBrowserMod.Harmony;
using LobbyBrowserMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LobbyBrowserMod.Core
{
    public static class LobbyBrowser
    {
        public static event Action OnUpdate;

        private static LobbyBrowseResult _lastServerResult;
        private static Dictionary<int, LobbyAnnounceInfo> _lobbyObjects;
        private static List<LobbyAnnounceInfo> _lobbiesOnPage;
        private static int _offset;

        public static async Task FullRefresh()
        {
            _lastServerResult = null;
            _lobbyObjects = new Dictionary<int, LobbyAnnounceInfo>(10);
            _lobbiesOnPage = new List<LobbyAnnounceInfo>(10);

            await LoadPage(0);
        }

        public static async Task LoadPage(int offset)
        {
            // Send API request
            var result = await MasterServerApi.Browse(offset);

            // Update state
            _offset = offset;
            _lastServerResult = result;

            // If we got results, process and index the lobby info
            var nextLobbiesOnPage = new List<LobbyAnnounceInfo>();

            if (_lastServerResult != null)
            {
                foreach (var lobby in _lastServerResult.Lobbies)
                {
                    _lobbyObjects[lobby.Id.Value] = lobby;
                    nextLobbiesOnPage.Add(lobby);
                }
            }

            _lobbiesOnPage = nextLobbiesOnPage;

            // Fire update event for the UI
            OnUpdate.Invoke();
        }

        public static bool ConnectionOk
        {
            get
            {
                return _lastServerResult != null;
            }
        }

        public static bool AnyResults
        {
            get
            {
                return _lastServerResult != null && _lastServerResult.Lobbies.Count > 0;
            }
        }

        public static int TotalResultCount
        {
            get
            {
                return _lastServerResult != null ? _lastServerResult.Count : 0;
            }
        }

        public static int PageNumber
        {
            get
            {
                return (int)Math.Floor((double)_offset / (double)PageSize);
            }
        }

        public static int PageCount
        {
            get
            {
                return (int)Math.Floor((double)TotalResultCount / (double)PageSize);
            }
        }

        public static int PageSize
        {
            get
            {
                return _lastServerResult != null ? _lastServerResult.Limit : 10;
            }
        }

        public static List<LobbyAnnounceInfo> LobbiesOnPage
        {
            get
            {
                return new List<LobbyAnnounceInfo>(_lobbiesOnPage);
            }
        }
    }
}
