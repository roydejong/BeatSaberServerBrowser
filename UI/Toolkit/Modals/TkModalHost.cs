using System;
using System.Collections;
using HMUI;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Screen = HMUI.Screen;

namespace ServerBrowser.UI.Toolkit.Modals
{
    public class TkModalHost : MonoBehaviour
    {
        [Inject] private readonly DiContainer _diContainer = null!;
        [Inject] private readonly MaterialAccessor _materialAccessor = null!;
        [Inject] private readonly GameplaySetupViewController _gameplaySetupViewController = null!;
        
        private CanvasGroup _canvasGroup = null!;
        private TkModalView? _modalView = null;
        private GameObject? _modalRoot = null;
        private GameObject? _blocker = null;

        public T ShowModal<T>() where T : TkModalView
        {
            if (_modalView != null)
                CloseModal();

            // Modal root: spans the entire screen 
            if (_modalRoot == null)
            {
                _modalRoot = new GameObject("TkModalHost")
                {
                    layer = LayoutContainer.UiLayer
                };
                _modalRoot.transform.SetParent(transform, false);
            }
            _modalRoot.transform.SetAsLastSibling();
            
            var parentScreen = gameObject.GetComponentInParent<Screen>();
            var parentRect = (parentScreen.transform as RectTransform)!.rect;
            
            var canvasGroup = _modalRoot.GetOrAddComponent<CanvasGroup>();
            canvasGroup.ignoreParentGroups = true;

            var rootRect = _modalRoot.GetOrAddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(parentRect.width, parentRect.height);
            
            // Blocker button: covers entire screen, blocks input to the base view, closes the modal when clicked
            if (_blocker == null)
            {
                _blocker = new GameObject("Blocker")
                {
                    layer = LayoutContainer.UiLayer
                };
                _blocker.transform.SetParent(_modalRoot.transform, false);

                var blockerCanvasGroup = _blocker.AddComponent<CanvasGroup>();
                blockerCanvasGroup.ignoreParentGroups = true;
                blockerCanvasGroup.interactable = true;
                blockerCanvasGroup.blocksRaycasts = true;

                // this is a load bearing invisible image
                var blockerImage = _blocker.AddComponent<Image>();
                blockerImage.color = new Color(0, 0f, 0f, 0f);

                var blockerButton = _blocker.AddComponent<Button>();
                blockerButton.onClick.AddListener(CloseModal);
            }

            var blockerRect = (_blocker.transform as RectTransform)!;
            blockerRect.sizeDelta = new Vector2(parentRect.width, parentRect.height);
            _blocker.SetActive(true);

            // Modal view itself: constructs its own layout, we only set up the size
            var modalTypeName = typeof(T).Name;
            var modalGo = new GameObject(modalTypeName)
            {
                layer = LayoutContainer.UiLayer
            };
            modalGo.transform.SetParent(_modalRoot.transform, false);
            
            _modalView = _diContainer.InstantiateComponent<T>(modalGo);

            var modalRect = modalGo.GetOrAddComponent<RectTransform>();
            modalRect.sizeDelta = new Vector2(_modalView.ModalWidth, _modalView.ModalHeight);
            
            _modalView.Initialize();
            
            RunOpenPanelAnimation(_modalView.gameObject);
            
            FadeOutBaseView();

            return (T)_modalView;
        }

        public void CloseModal()
        {
            if (_blocker != null)
                _blocker.SetActive(false);
            
            if (_modalView == null)
                return;

            var closeGo = _modalView.gameObject;
            RunClosePanelAnimation(closeGo, () =>
            {
                Destroy(closeGo);
            });
            
            _modalView.InvokeModalClosed();
            _modalView = null;

            FadeInBaseView();
        }

        #region Panel Animation

        public void RunOpenPanelAnimation(GameObject panelGo, Action? finishCallback = null)
        {
            var presentAnim = _gameplaySetupViewController._colorsOverrideSettingsPanelController
                ._presentPanelAnimation;
            
            panelGo.SetActive(true);
            presentAnim.ExecuteAnimation(panelGo, finishCallback);
        }

        public void RunClosePanelAnimation(GameObject panelGo, Action? finishCallback = null)
        {
            var closeAnim = _gameplaySetupViewController._colorsOverrideSettingsPanelController
                ._dismissPanelAnimation;    
            closeAnim.ExecuteAnimation(panelGo, finishCallback);
        }

        #endregion
        
        #region Base View Fade

        public void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        public void OnEnable()
        {
            _canvasGroup.alpha = 1f;
        }

        public void OnDisable()
        {
            _canvasGroup.alpha = 1f;
        }
        
        public void FadeOutBaseView()
        {
            StopAllCoroutines();
            StartCoroutine(FadeInOutCoroutine(0.25f));
        }
        
        public void FadeInBaseView()
        {
            StopAllCoroutines();
            StartCoroutine(FadeInOutCoroutine(1f));
        }
        
        public IEnumerator FadeInOutCoroutine(float targetAlpha)
        {
            const float duration = .1f;
            
            var startAlpha = _canvasGroup.alpha;
            var startTime = Time.time;
        
            while (true)
            {
                var t = Mathf.Clamp01((Time.time - startTime) / duration);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                
                if (t >= 1f)
                    break;
                
                yield return new WaitForEndOfFrame();    
            }
        }

        #endregion

        #region Static API
        
        public static T ShowModal<T>(ViewController host, DiContainer diContainer) where T : TkModalView
        {
            var modalHost = host.gameObject.GetComponent<TkModalHost>();

            if (modalHost == null)
                modalHost = diContainer.InstantiateComponent<TkModalHost>(host.gameObject);
            
            return modalHost.ShowModal<T>();
        }

        public static void CloseAnyModal(ViewController host)
        {
            var modalHost = host.gameObject.GetComponent<TkModalHost>();
            
            if (modalHost != null)
                modalHost.CloseModal();
        }
        
        #endregion
    }
}