using System;
using System.Collections;
using ServerBrowser.Assets;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.UI.Toolkit.Modals;
using TMPro;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Browser.Modals
{
    public class ServerCodeModalView : TkModalView
    {
        [Inject] private readonly LayoutBuilder _layoutBuilder = null!;

        public override float ModalWidth => 50f;
        public override float ModalHeight => 30f;

        private LayoutContainer _rootWrap = null!;
        private LayoutContainer _container = null!;
        private TkTextInputField _inputField = null!;
        private bool _okPressed = false;

        public event Action<string?> FinishedEvent;

        public override void Initialize()
        {
            _rootWrap = new LayoutContainer(_layoutBuilder, transform, false);
            _rootWrap.RectTransform.anchorMin = new Vector2(.5f, .7f);
            _rootWrap.RectTransform.anchorMax = new Vector2(.5f, .7f);

            _container = _rootWrap.AddVerticalLayoutGroup("Content",
                padding: new RectOffset(4, 4, 4, 8),
                expandChildWidth: true, expandChildHeight: false);
            _container.SetBackground("round-rect-panel");

            _container.AddText("Join by Code", textAlignment: TextAlignmentOptions.Center);
            
            _container.InsertMargin(-1f, 1f);
 
            _inputField = _container.AddTextInputField("Server Code", iconSprite: Sprites.Lock);
            _inputField.SetKeyboardOffset(new Vector3(0, -12f, 0));
            _inputField.KeyboardOkPressedEvent += HandleKeyboardOk;

            StartCoroutine(AutoOpenCoroutine());
        }

        public IEnumerator AutoOpenCoroutine()
        {
            yield return new WaitForSeconds(.1f);
            yield return new WaitForEndOfFrame();
            _inputField.OpenKeyboard();
        }

        private void HandleKeyboardOk(string value)
        {
            _okPressed = true;
            CloseModal();
            FinishedEvent?.Invoke(value);
        }
    }
}