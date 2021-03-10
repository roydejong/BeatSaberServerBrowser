using ServerBrowser.Assets;
using ServerBrowser.Core;
using ServerBrowser.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ServerBrowser.Core
{
    public static class HostedGameBrowser
    {
        public static event Action OnUpdate;

        private static ServerBrowseResult _lastServerResult;
        private static Dictionary<int, HostedGameData> _lobbyObjects;
        private static List<HostedGameData> _lobbiesOnPage;
        private static int _offset;

        public static async Task FullRefresh(HostedGameFilters filters)
        {
            _lastServerResult = null;
            _lobbyObjects = new Dictionary<int, HostedGameData>(10);
            _lobbiesOnPage = new List<HostedGameData>(10);

            await LoadPage(0, filters);
        }

        public static async Task LoadPage(int offset, HostedGameFilters filters)
        {
            // Send API request
            var result = await BSSBMasterAPI.Browse(offset, filters);

            // Update state
            _offset = offset;
            _lastServerResult = result;

            // If we got results, process and index the lobby info
            var nextLobbiesOnPage = new List<HostedGameData>();

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

        public static bool AnyResultsOnPage
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

        public static int PageIndex
        {
            get
            {
                return (int)Math.Floor((double)_offset / (double)PageSize);
            }
        }

        public static int TotalPageCount
        {
            get
            {
                return (int)Math.Ceiling((double)TotalResultCount / (double)PageSize);
            }
        }

        public static int PageSize
        {
            get
            {
                return _lastServerResult != null ? _lastServerResult.Limit : 10;
            }
        }

        public static List<HostedGameData> LobbiesOnPage
        {
            get
            {
                return new List<HostedGameData>(_lobbiesOnPage);
            }
        }

        public static string ServerMessage
        {
            get
            {
                return _lastServerResult?.Message;
            }
        }
    }
}
