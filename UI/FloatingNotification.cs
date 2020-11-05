using HMUI;
using System.Linq;
using UnityEngine;
using VRUIControls;

namespace ServerBrowser.UI
{
    public class FloatingNotification : MonoBehaviour
    {
        #region Enums
        public enum NotificationStyle
        {
            Blue = 0,
            Red = 1,
            Yellow = 2
        }

        private enum NotificationStep
        {
            Hidden,
            Appearing,
            Normal,
            Disappearing
        }
        #endregion

        #region Creation / Instance
        private const string GameObjectName = "ServerBrowserFloatingNotification";

        public static FloatingNotification Instance
        {
            get;
            private set;
        }

        public static FloatingNotification SetUp()
        {
            if (Instance == null)
            {
                Instance = new GameObject(GameObjectName).AddComponent<FloatingNotification>();
                Instance.DismissMessage();
            }

            return Instance;
        }
        #endregion

        #region API
        private bool _requestedStart = false;
        private string _title;
        private string _message;
        private float _time;
        private NotificationStyle _style;

        public void ShowMessage(string title, string message, NotificationStyle style = NotificationStyle.Blue, float time = 5.0f)
        {
            Plugin.Log.Info($" -----> ShowMessage: {title}, {message}, {time} sec");

            _requestedStart = true;

            _title = title;
            _message = message;
            _style = style;
            _time = time;
            _currentStep = NotificationStep.Hidden;

            gameObject.SetActive(true);
        }

        public void DismissMessage()
        {
            _requestedStart = false;
            _currentStep = NotificationStep.Hidden;
        }
        #endregion

        #region Unity / Rendering
        private NotificationStep _currentStep = NotificationStep.Hidden;

        private Transform _clonedMainScreen;
        private CanvasGroup _canvasGroup;
        private GameObject _levelBar;
        private ImageView _bgImage;
        private CurvedTextMeshPro _titleTextMesh;
        private CurvedTextMeshPro _subTitleTextMesh;

        private const float ANIMATE_TIME = 0.15f;
        private const float ANIMATE_Y_OFFSET = -0.5f;

        private float _updateTimerTally = 0.0f;
        private float _basePosY = 3.0f;

        private void Awake()
        {
            Plugin.Log.Info($" -----> AWAKE");

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
            _clonedMainScreen.name = "SBFNMainScreen";
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
            _levelBar = GameObject.Find("ServerBrowserFloatingNotification/SBFNMainScreen/CenterStageScreenController/NextLevel/BeatmapWithModifiersNonEditable/BeatmapSelection/LevelBarSimple");
            Destroy(_levelBar.transform.Find("BeatmapDataContainer").gameObject);

            _bgImage = _levelBar.transform.parent.GetComponentInChildren<ImageView>();
            
            var titleText = _levelBar.transform.Find("SongNameText");
            _titleTextMesh = titleText.GetComponent<CurvedTextMeshPro>();
            _titleTextMesh.text = "NOTIFICATION_TITLE";

            var subTitleText = _levelBar.transform.Find("AuthorNameText");
            _subTitleTextMesh = subTitleText.GetComponent<CurvedTextMeshPro>();
            _subTitleTextMesh.text = "NOTIFICATION_MESSAGE";

            _levelBar.SetActive(true);

            // Keep our object alive across scenes so we can display ingame
            DontDestroyOnLoad(gameObject);
        }

        private void PresentNextMessage()
        {
            _updateTimerTally = 0.0f;
            _currentStep = NotificationStep.Appearing;

            _titleTextMesh.SetText(_title);
            _subTitleTextMesh.SetText(_message);

            switch (_style)
            {
                default:
                case NotificationStyle.Blue:
                    _bgImage.color = new Color(52f / 255f, 31f / 255f, 151f / 255f);
                    break;
                case NotificationStyle.Red:
                    _bgImage.color = new Color(238f / 255f, 82f / 255f, 83f / 255f);
                    break;
                case NotificationStyle.Yellow:
                    _bgImage.color = new Color(254f / 255f, 202f / 255f, 87f / 255f);
                    break;
            }

            _requestedStart = false;
        }

        private void Update()
        {
            _updateTimerTally += Time.deltaTime;

            if (_currentStep == NotificationStep.Hidden)
            {
                // We are idle, or just finished displaying a notification
                if (_requestedStart)
                {
                    // New notification requested, begin appear animation from zero
                    PresentNextMessage();
                }
                else
                {
                    // Nothing to show, suicide
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
