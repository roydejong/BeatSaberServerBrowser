using System;
using ServerBrowser.Data;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.UI.Toolkit.Modals;
using ServerBrowser.UI.Toolkit.Wrappers;
using TMPro;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Browser.Modals
{
    public class ServerModalView : TkModalView
    {
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;

        public override float ModalWidth => 65f;
        public override float ModalHeight => 50f;

        private ServerRepository.ServerInfo? _serverInfo = null;

        private LayoutContainer _container = null!;
        private TkText _titleText = null!;
        private TkButton _connectButton = null!;
        
        private TkTableView.TableRow _rowGameMode = null!; 
        private TkTableView.TableRow _rowPlayerCount = null!; 
        private TkTableView.TableRow _rowLobbyStatus = null!;

        public event Action<ServerRepository.ServerInfo>? ConnectClickedEvent;

        public override void Initialize()
        {
            var wrap = new LayoutContainer(_layoutBuilder, transform, false);
            
            _container = wrap.AddVerticalLayoutGroup("Content",
                padding: new RectOffset(4, 4, 4, 4),
                expandChildWidth: true, expandChildHeight: false);
            _container.SetBackground("round-rect-panel");
            
            _titleText = _container.AddText("Server", textAlignment: TextAlignmentOptions.Center);
            
            _container.InsertMargin(-1f, 4f);

            var tableView = _container.AddTableView();
            _rowGameMode = tableView.AddRow("Game mode");
            _rowPlayerCount = tableView.AddRow("Player count");
            _rowLobbyStatus = tableView.AddRow("Lobby status");

            _connectButton = _container.AddButton("Connect", primary: true,
                paddingVertical: 3, paddingHorizontal: 6, clickAction: () =>
                {
                    if (_serverInfo != null)
                        ConnectClickedEvent?.Invoke(_serverInfo);
                });
        }

        public void SetData(ServerRepository.ServerInfo serverInfo)
        {
            _serverInfo = serverInfo;
            
            _titleText.SetText(serverInfo.ServerName);
            
            _rowGameMode.Value = serverInfo.GameModeName;
            _rowPlayerCount.Value = $"{serverInfo.PlayerCount}/{serverInfo.PlayerLimit}";
            _rowLobbyStatus.Value = serverInfo.InGameplay ? "Playing level" : "In lobby";
        }
    }
}