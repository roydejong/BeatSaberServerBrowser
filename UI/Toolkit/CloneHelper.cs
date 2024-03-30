using System;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace ServerBrowser.UI.Toolkit
{
    [UsedImplicitly]
    public class CloneHelper
    {
        [Inject] private readonly DiContainer _diContainer = null!;

        public GameObject CreateEmpty(Transform parent, string name)
        {
            var gameObject = new GameObject(name)
            {
                layer = LayoutContainer.UiLayer
            };
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        public T CloneTemplate<T>(T template, Transform parent, string name) where T : MonoBehaviour
        {
            var cloneObject = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
            cloneObject.name = name;
            
            var component = cloneObject.GetComponent<T>();
            _diContainer.Inject(component);
            return component;
        }

        #region Settings controls

        private T GetSettingsViewController<T>() where T : ViewController
        {
            var mainSettingsMenuViewController = _diContainer.Resolve<MainSettingsMenuViewController>();

            foreach (var settingsSubMenuInfo in mainSettingsMenuViewController._settingsSubMenuInfos)
                if (settingsSubMenuInfo.viewController is T controller)
                    return controller;

            throw new InvalidOperationException(
                $"Could not resolve settings controller: {typeof(T).Name} - Mod may need to be updated");
        }

        private OtherSettingsViewController GetOtherSettingsViewController() =>
            GetSettingsViewController<OtherSettingsViewController>();

        public ToggleWithCallbacks GetToggleTemplate() =>
            (GetOtherSettingsViewController()._hideExplicitToggle as ToggleWithCallbacks) ??
            throw new InvalidOperationException(
                $"Could not resolve Toggle template - Mod may need to be updated");

        public SimpleTextDropdown GetDropdownTemplate() =>
            GetOtherSettingsViewController().transform.Find("Content/LanguageDropdown/SimpleTextDropDown")
                ?.GetComponent<SimpleTextDropdown>() ??
            throw new InvalidOperationException(
                $"Could not resolve Dropdown template - Mod may need to be updated");

        #endregion
    }
}