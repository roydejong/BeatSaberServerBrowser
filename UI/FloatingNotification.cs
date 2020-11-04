﻿using HMUI;
using System.Linq;
using UnityEngine;
using VRUIControls;

namespace ServerBrowser.UI
{
    public class FloatingNotification : MonoBehaviour
    {
        #region Creation / Instance
        private const string GameObjectName = "ServerBrowserFloatingNotification";

        private static FloatingNotification _instance;
        public static FloatingNotification Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Create();
                }

                return _instance; 
            }
        }

        private static FloatingNotification Create()
        {
            return new GameObject(GameObjectName).AddComponent<FloatingNotification>();
        }
        #endregion

        #region API
        private bool _requestedStart = false;
        private string _title;
        private string _message;
        private float _time;

        public void ShowMessage(string title, string message, float time = 5.0f)
        {
            _requestedStart = true;
            _title = title;
            _message = message;
            _time = time;

            gameObject.SetActive(true);
        }
        #endregion

        #region Unity / Rendering
        enum NotificationStep
        {
            Hidden,
            Appearing,
            Normal,
            Disappearing
        }

        private NotificationStep _currentStep = NotificationStep.Hidden;

        private Transform _clonedMainScreen;
        private CanvasGroup _canvasGroup;
        private GameObject _levelBar;
        private ImageView _bgImage;
        private CurvedTextMeshPro _titleTextMesh;
        private CurvedTextMeshPro _subTitleTextMesh;

        private float _basePosY = 3.0f;

        private void Awake()
        {
            ////////////////////////////////////////////////////////////////////////////////////////
            /// Hello!
            /// Please don't look at this code because cloning such a huge object is kinda gross
            /// 
            /// It was fun doing this in a really ugly way
            /// Now eventually I'll try to get it to work without ugliness :)
            ////////////////////////////////////////////////////////////////////////////////////////

            // Clone the "main screen" for the lobby, normally used to show currently selected song in the distance
            var screenController = Resources.FindObjectsOfTypeAll<CenterStageScreenController>().First();
            var mainScreen = screenController.transform.parent;

            _clonedMainScreen = UnityEngine.Object.Instantiate(mainScreen);
            _clonedMainScreen.parent = gameObject.transform;

            _canvasGroup = _clonedMainScreen.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.enabled = true;

            // Set base position
            _clonedMainScreen.transform.position = new Vector3(0, _basePosY, 3.0f);

            // Activate all child objects, filtering out game objects we're not interested in
            foreach (var tr in _clonedMainScreen.GetComponentsInChildren<Transform>())
            {
                switch (tr.gameObject.name)
                {
                    default:
                        tr.gameObject.SetActive(true);
                        break;
                    case "NextLevelBasePosition":
                    case "NextLevelCountdownPosition":
                    case "Title":
                    case "PlaceholderText":
                    case "ModifierSelection":
                    case "Countdown":
                        Destroy(tr.gameObject);
                        break;
                }
            }

            _clonedMainScreen.gameObject.SetActive(true);

            // Destroy any graphic raycasters, our notifs aren't clickable and they cause issues
            foreach (var vgr in _clonedMainScreen.GetComponentsInChildren<VRGraphicRaycaster>())
            {
                Destroy(vgr);
            }

            // Grab the level bar, which is what will hold our notification content
            _levelBar = GameObject.Find("ServerBrowserFloatingNotification/MainScreen(Clone)/CenterStageScreenController/NextLevel/BeatmapWithModifiersNonEditable/BeatmapSelection/LevelBarSimple");
            _levelBar.SetActive(true);

            _bgImage = _levelBar.transform.parent.GetComponentInChildren<ImageView>();
            _bgImage.color = new Color(52f/255f, 31f/255f, 151f/255f);
            
            var titleText = _levelBar.transform.Find("SongNameText");
            _titleTextMesh = titleText.GetComponent<CurvedTextMeshPro>();
            _titleTextMesh.text = "NOTIFICATION_TITLE";

            var subTitleText = _levelBar.transform.Find("AuthorNameText");
            _subTitleTextMesh = subTitleText.GetComponent<CurvedTextMeshPro>();
            _subTitleTextMesh.text = "NOTIFICATION_MESSAGE";

            var modifierIndicator = _levelBar.transform.Find("BeatmapDataContainer");
            modifierIndicator.gameObject.SetActive(false);

            // Keep our object alive across scenes so we can display ingame
            DontDestroyOnLoad(gameObject);
        }

        private const float ANIMATE_TIME = 0.15f;
        private const float ANIMATE_Y_OFFSET = -0.5f;

        private float _updateTimerTally = 0.0f;

        private void Update()
        {
            _updateTimerTally += Time.deltaTime;

            if (_currentStep == NotificationStep.Hidden)
            {
                // We are idle, or just finished displaying a notification
                if (_requestedStart)
                {
                    // New notification requested, begin appear animation from zero
                    _updateTimerTally = 0.0f;
                    _currentStep = NotificationStep.Appearing;

                    _titleTextMesh.SetText(_title);
                    _subTitleTextMesh.SetText(_message);

                    _requestedStart = false;
                }
                else
                {
                    // We have nothing left to do, end our suffering
                    gameObject.SetActive(false);
                }

                return;
            }

            float bgAlpha = 1.0f;
            float yOffset = 0.0f;

            if (_currentStep == NotificationStep.Appearing)
            {
                if (_updateTimerTally < ANIMATE_TIME)
                {
                    // Fading in for FADE_TIME
                    bgAlpha = 1.0f * (_updateTimerTally / ANIMATE_TIME);
                    yOffset = ANIMATE_Y_OFFSET - (ANIMATE_Y_OFFSET * (_updateTimerTally / ANIMATE_TIME));
                }
                else
                {
                    // FADE_TIME passed; we are now visible, start counting again to disappear per _time
                    bgAlpha = 1.0f;
                    yOffset = 0.0f;

                    _currentStep = NotificationStep.Normal;
                    _updateTimerTally = 0.0f;
                }
            }
            else if (_currentStep == NotificationStep.Disappearing)
            {
                if (_updateTimerTally < ANIMATE_TIME)
                {
                    // Fading out for FADE_TIME
                    bgAlpha = 1.0f - (1.0f * (_updateTimerTally / ANIMATE_TIME));
                    yOffset = (-ANIMATE_Y_OFFSET * (_updateTimerTally / ANIMATE_TIME));
                }
                else
                {
                    // FADE_TIME passed; we are now done
                    bgAlpha = 0.0f;
                    yOffset = -ANIMATE_Y_OFFSET;

                    _currentStep = NotificationStep.Hidden;
                    _updateTimerTally = 0.0f;
                }
            }
            else if (_currentStep == NotificationStep.Normal)
            {
                // Wait for _time to pass then begin to disappear
                bgAlpha = 1.0f;

                if (_updateTimerTally >= _time)
                {
                    _currentStep = NotificationStep.Disappearing;
                    _updateTimerTally = 0.0f;
                }
            }

            if (bgAlpha != _canvasGroup.alpha)
            {
                _canvasGroup.alpha = bgAlpha;
            }

            var targetPosY = _basePosY + yOffset;

            if (_clonedMainScreen.position.y != targetPosY)
            {
                _clonedMainScreen.position = new Vector3(_clonedMainScreen.position.x, targetPosY, _clonedMainScreen.position.z);
            }
        }
        #endregion
    }
}