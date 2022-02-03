using System.Collections;
using System.Collections.Generic;
using HMUI;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ServerBrowser.UI.Components
{
    public class BssbFloatingAlert : MonoBehaviour, IInitializable
    {
        private const float BasePosX = 0f;
        private const float BasePosY = 1.5f;
        private const float BasePosZ = 4.2f;
        private const float AnimationDuration = .15f;
        private const float AnimationOffsetY = -.5f;
        private const float DisplayTime = 5f;

        public static Vector3 BasePos => new Vector3(BasePosX, BasePosY, BasePosZ);
        
        [Inject] private readonly DiContainer _diContainer = null!;
        [Inject] private readonly SiraLog _log = null!;
        
        private BssbLevelBarClone? _levelBar;
        private CanvasGroup? _canvasGroup;
        private Queue<NotificationData> _pendingNotifications;
        private bool _isPresenting;

        public BssbFloatingAlert()
        {
            _pendingNotifications = new();
        }

        #region Lifecycle
        public void Initialize()
        {
        }

        internal void OnMenu()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.scaleFactor = 3.44f;
            canvas.referencePixelsPerUnit = 10;
            canvas.planeDistance = 100;

            gameObject.AddComponent<CanvasRenderer>();
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<CurvedCanvasSettings>();
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _levelBar = BssbLevelBarClone.Create(_diContainer, transform);

            StopPresentingImmediate();
        }

        public void Update()
        {
            // If we are not presenting a notification, and have a pending one, do so now
            // If the queue is empty, self-disable, we are done for now
            
            if (_isPresenting)
                return;

            if (_pendingNotifications.Count == 0)
            {
                _log.Info("No more notifications to display, disabling");
                gameObject.SetActive(false);
                return;
            }
             
            PresentNotificationImmediate(_pendingNotifications.Dequeue());
        }

        #endregion

        public void StopPresentingImmediate()
        {
            StopAllCoroutines();
            
            if (_canvasGroup is not null)
                _canvasGroup.alpha = 0f;

            transform.position = BasePos;
            transform.rotation = Quaternion.identity;
            transform.localScale = new Vector3(.04f, .04f, .04f);

            _isPresenting = false;
        }

        public void PresentNotification(NotificationData notification)
        {
            _pendingNotifications.Enqueue(notification);
            
            if (!isActiveAndEnabled)
                gameObject.SetActive(true);
            
            // Update will present next notification in queue when appropriate
        }
        
        public void PresentNotificationImmediate(NotificationData notification)
        {
            if (_levelBar is null || _canvasGroup is null)
                return;
            
            StopPresentingImmediate();
            
            gameObject.SetActive(true);
            
            _levelBar.SetBackgroundStyle(notification.BackgroundStyle);
            _levelBar.SetImageSprite(notification.Sprite);
            _levelBar.SetText(notification.Title, notification.MessageText);
            
            _log.Info($"Presenting notification ({notification.Title}, {notification.MessageText})");

            _isPresenting = true;
            
            StartCoroutine(nameof(AnimateIn));
        }

        #region Animation/Coroutines
        private IEnumerator AnimateIn()
        {
            if (_canvasGroup is null)
                yield break;

            var runTime = 0f;
            
            while (runTime < AnimationDuration)
            {
                runTime += Time.deltaTime;
                var animationProgress = (runTime / AnimationDuration);
                
                _canvasGroup.alpha = 1.0f * animationProgress;
                var yOffset = AnimationOffsetY - (AnimationOffsetY * (runTime / AnimationDuration));
                transform.position = new Vector3(BasePosX, BasePosY + yOffset, BasePosZ); 
                
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            transform.position = BasePos;

            // Wait for some time then animate out
            yield return new WaitForSeconds(DisplayTime);
            StartCoroutine(nameof(AnimateOut));
        }
        
        private IEnumerator AnimateOut()
        {
            if (_canvasGroup is null)
                yield break;

            var runTime = 0f;
            
            while (runTime < AnimationDuration)
            {
                runTime += Time.deltaTime;
                var animationProgress = (runTime / AnimationDuration);
                
                _canvasGroup.alpha = 1.0f - (1.0f * (animationProgress));
                var yOffset =  (-AnimationOffsetY * animationProgress);
                transform.position = new Vector3(BasePosX, BasePosY + yOffset, BasePosZ); 
                
                yield return null;
            }

            _canvasGroup.alpha = 0f;

            // Animated out and no longer visible, end of presentation
            yield return new WaitForEndOfFrame();
            StopPresentingImmediate();
        }

        #endregion

        public class NotificationData
        {
            public readonly Sprite? Sprite;
            public readonly string Title;
            public readonly string MessageText;
            public readonly BssbLevelBarClone.BackgroundStyle BackgroundStyle;

            public NotificationData(Sprite? sprite, string title, string messageText,
                BssbLevelBarClone.BackgroundStyle backgroundStyle = BssbLevelBarClone.BackgroundStyle.ColorfulGradient)
            {
                Sprite = sprite;
                Title = title;
                MessageText = messageText;
                BackgroundStyle = backgroundStyle;
            }
        }
    }
}