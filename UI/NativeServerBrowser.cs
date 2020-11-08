using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.UI.Components;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.Parser;
using System.Text;

namespace ServerBrowser.UI
{
    public class NativeServerBrowser : MonoBehaviour
    {
        #region Creation / Instance
        private const string GameObjectName = "NativeServerBrowserButItsTheMod";

        public static NativeServerBrowser Instance
        {
            get;
            private set;
        }

        public static GameServerBrowserViewController ViewController
        {
            get;
            private set;
        }

        public static NativeServerBrowser SetUp()
        {
            if (ViewController == null)
            {
                ViewController = Resources.FindObjectsOfTypeAll<GameServerBrowserViewController>().FirstOrDefault();
            }

            if (Instance == null)
            {
                Instance = ViewController.gameObject.AddComponent<NativeServerBrowser>();
            }

            return Instance;
        }
        #endregion

        #region Core / Data
        private string _searchQuery = null;

        private void OnEnable()
        {
            MpModeSelection.SetTitle("Server Browser");

            HostedGameBrowser.OnUpdate += OnBrowserUpdate;
            _tableView.joinButtonPressedEvent += OnJoinPressed;

            DoFullRefresh();
        }

        private void OnDisable()
        {
            HostedGameBrowser.OnUpdate -= OnBrowserUpdate;
            _tableView.joinButtonPressedEvent -= OnJoinPressed;
        }

        private void DoFullRefresh()
        {
            _refreshButton.interactable = false;
            _filterButton.interactable = false;

            _mainLoadingControl.ShowLoading("Loading server list");

            HostedGameBrowser.FullRefresh(_searchQuery);
        }

        private void OnBrowserUpdate()
        {
            _refreshButton.interactable = true;
            _filterButton.interactable = true;

            _tableView.SetData(HostedGameBrowser.LobbiesOnPage, true);

            UpdateLoadingControl();
            UpdateFilterButton();
        }

        private void UpdateLoadingControl()
        {
            if (!HostedGameBrowser.ConnectionOk)
            {
                _mainLoadingControl.ShowText("Failed to get server list", true);
            }
            else if (!HostedGameBrowser.AnyResults)
            {
                var isSearching = !String.IsNullOrEmpty(_searchQuery);

                if (isSearching)
                {
                    _mainLoadingControl.ShowText("No servers found matching your search", false);
                }
                else
                {
                    _mainLoadingControl.ShowText("No servers found", true);
                }
            }
            else
            {
                _mainLoadingControl.Hide();
            }
        }

        private void UpdateFilterButton()
        {
            var nextLabel = new StringBuilder();

            // Current constraints list
            if (!String.IsNullOrEmpty(_searchQuery))
            {
                nextLabel.Append($"Search: \"{_searchQuery}\"");
            }
            else
            {
                nextLabel.Append("All servers");
            }

            // No MpEx?
            if (!MpSession.GetLocalPlayerHasMultiplayerExtensions())
            {
                nextLabel.Append(", no modded games");
            }

            // (X results)
            var resultCount = HostedGameBrowser.TotalResultCount;
            var resultUnit = resultCount == 1 ? "result" : "results";
            nextLabel.Append($" ({resultCount} {resultUnit})");

            // Apply
            _filterButtonLabel.SetText(nextLabel.ToString());
        }

        private void OnJoinPressed(INetworkPlayer game)
        {
            ((HostedGameData)game).Join();
        }

        private void OnModalKeyboardSubmit(string searchValue)
        {
            if (_searchQuery != searchValue)
            {
                _searchQuery = searchValue;
                DoFullRefresh();
            }
        }
        private void OnRefreshPressed()
        {
            DoFullRefresh();
        }
        #endregion

        #region Components
        private Button _createServerButton;
        private Button _filterButton;
        private CurvedTextMeshPro _filterButtonLabel;
        private Button _refreshButton;
        private LoadingControl _mainLoadingControl;
        private GameServersListTableView _tableView;
        #endregion

        #region Awake (UI Setup)
        private ModalKeyboard _modalKeyboard;

        private void Awake()
        {
            // Create ModalKeyboard (BSML)
            var modalKeyboardTag = new ModalKeyboardTag();
            var modalKeyboardObj = modalKeyboardTag.CreateObject(transform);

            _modalKeyboard = modalKeyboardObj.GetComponent<ModalKeyboard>();
            _modalKeyboard.clearOnOpen = false;
            _modalKeyboard.keyboard.EnterPressed += OnModalKeyboardSubmit;

            // Create server button
            var createServerButtonTransform = transform.Find("CreateServerButton");
            createServerButtonTransform.localPosition = new Vector3(-76.50f, 40.0f, 0.0f);

            _createServerButton = transform.Find("CreateServerButton").GetComponent<Button>();
            _createServerButton.onClick.AddListener(delegate
            {
                MpModeSelection.OpenCreateServerMenu();
            });

            // Move the top-right loading control up, so the refresh button aligns properly
            (transform.Find("Filters/SmallLoadingControl") as RectTransform).localPosition = new Vector3(62.0f, 3.5f, 0.0f);

            // Resize the filters bar so it doesn't overlap the refresh button
            var filterButtonTransform = (transform.Find("Filters/FilterButton") as RectTransform);
            filterButtonTransform.sizeDelta = new Vector2(-11.0f, 10.0f);
            filterButtonTransform.offsetMax = new Vector2(-11.0f, 5.0f);

            _filterButton = filterButtonTransform.GetComponent<Button>();
            _filterButton.onClick.AddListener(delegate
            {
                _modalKeyboard.keyboard.KeyboardText.text = !String.IsNullOrEmpty(_searchQuery) ? _searchQuery : "";
                //_modalKeyboard.keyboard.KeyboardText.fontSize = 4;

                _modalKeyboard.modalView.Show(true, true, null);
            });

            // Filters lable
            _filterButtonLabel = transform.Find("Filters/FilterButton/Content/Text").GetComponent<CurvedTextMeshPro>();
            _filterButtonLabel.text = "Hello world!";

            // Hide top-right loading spinners
            Destroy(transform.Find("Filters/SmallLoadingControl/LoadingContainer").gameObject);
            Destroy(transform.Find("Filters/SmallLoadingControl/DownloadingContainer").gameObject);

            // Refresh button (add listener, make visible)
            var smallLoadingControl = transform.Find("Filters/SmallLoadingControl").GetComponent<LoadingControl>();
            smallLoadingControl.didPressRefreshButtonEvent += OnRefreshPressed;

            var refreshContainer = smallLoadingControl.transform.Find("RefreshContainer");
            refreshContainer.gameObject.SetActive(true);

            _refreshButton = refreshContainer.Find("RefreshButton").GetComponent<Button>();

            // Change "Music Packs" table header to "Type"
            transform.Find("GameServersListTableView/GameServerListTableHeader/LabelsContainer/MusicPack").GetComponent<CurvedTextMeshPro>()
                .SetText("Type");

            // Main loading control
            _mainLoadingControl = transform.Find("GameServersListTableView/TableView/Viewport/MainLoadingControl").GetComponent<LoadingControl>();
            _mainLoadingControl.didPressRefreshButtonEvent += OnRefreshPressed;

            _mainLoadingControl.ShowLoading("Initializing");

            // Table view
            _tableView = transform.Find("GameServersListTableView").GetComponent<GameServersListTableView>();

            // Modify content cell prefab (add a background)
            var contentCellPrefab = _tableView.GetField<GameServerListTableCell, GameServersListTableView>("_gameServerListCellPrefab");

            var backgroundBase = Resources.FindObjectsOfTypeAll<ImageView>().First(x => x.gameObject?.name == "Background"
                && x.sprite != null && x.sprite.name.StartsWith("RoundRect10"));

            var backgroundClone = UnityEngine.Object.Instantiate(backgroundBase);
            backgroundClone.transform.SetParent(contentCellPrefab.transform, false);
            backgroundClone.transform.SetAsFirstSibling();
            backgroundClone.name = "Background";

            var backgroundTransform = backgroundClone.transform as RectTransform;
            backgroundTransform.anchorMin = new Vector2(0.0f, 0.0f);
            backgroundTransform.anchorMax = new Vector2(0.95f, 1.0f);
            backgroundTransform.offsetMin = new Vector2(0.5f, 0.0f);
            backgroundTransform.offsetMax = new Vector2(5.0f, 0.0f);
            backgroundTransform.sizeDelta = new Vector2(4.50f, 0.0f);

            var cellBackgroundHelper = contentCellPrefab.gameObject.AddComponent<CellBackgroundHelper>();
            cellBackgroundHelper.Cell = contentCellPrefab;
            cellBackgroundHelper.Background = backgroundClone;
        }
        #endregion
    }
}
