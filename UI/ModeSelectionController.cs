using System;
using HMUI;
using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI
{
    public class ModeSelectionController : IInitializable, IDisposable, IAffinity
    {
        [Inject] private readonly SiraLog _logger = null!;
        [Inject] private readonly MultiplayerModeSelectionFlowCoordinator _flowCoordinator = null!;
        [Inject] private readonly MultiplayerModeSelectionViewController _modeSelectionView = null!;

        private Button? _btnGameBrowser;

        public void Initialize()
        {
            _btnGameBrowser =
                _modeSelectionView.GetField<Button, MultiplayerModeSelectionViewController>("_gameBrowserButton");
        }

        public void Dispose()
        {
            if (_btnGameBrowser != null)
            {
                _btnGameBrowser.gameObject.SetActive(false);
                _btnGameBrowser = null;
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "DidActivate")]
        private void DidActivate(bool firstActivation)
        {
            if (_btnGameBrowser != null)
            {
                _btnGameBrowser.gameObject.SetActive(true);

                if (firstActivation)
                {
                    // Move up and enlarge the button a bit
                    var transform = _btnGameBrowser.gameObject.transform;
                    transform.position += new Vector3(0, .4f, 0);
                    transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                }
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerModeSelectionFlowCoordinator), 
            "HandleMultiplayerLobbyControllerDidFinish")]
        private bool DidPressMenuButton(MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            if (menuButton == MultiplayerModeSelectionViewController.MenuButton.GameBrowser)
            {
                // TODO Launch server browser UI
                return false;
            }

            return true;
        }
    }
}