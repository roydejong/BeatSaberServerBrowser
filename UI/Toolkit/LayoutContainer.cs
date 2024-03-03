using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using HMUI;
using ServerBrowser.UI.Toolkit.Wrappers;
using ServerBrowser.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit
{
    public class LayoutContainer
    {
        public const int UiLayer = 5;

        public readonly LayoutBuilder Builder;
        public readonly Transform Transform;
        public readonly GameObject GameObject;
        public readonly LayoutElement? LayoutElement;
        public readonly RectTransform RectTransform;

        public LayoutContainer(LayoutBuilder builder, Transform transform, bool isLayoutElement = true)
        {
            Builder = builder;
            Transform = transform;
            GameObject = transform.gameObject;
            if (isLayoutElement)
                LayoutElement = GameObject.GetOrAddComponent<LayoutElement>();
            RectTransform = (transform as RectTransform)!;
        }
        
        public float PreferredWidth
        {
            get
            {
                if (LayoutElement != null)
                    return LayoutElement.preferredWidth;
                return RectTransform.sizeDelta.x;
            }
            set
            {
                if (LayoutElement != null)
                    LayoutElement.preferredWidth = value;
                else
                    RectTransform.sizeDelta = new Vector2(value, RectTransform.sizeDelta.y);
            }
        }
        
        public float PreferredHeight
        {
            get
            {
                if (LayoutElement != null)
                    return LayoutElement.preferredHeight;
                return RectTransform.sizeDelta.y;
            }
            set
            {
                if (LayoutElement != null)
                    LayoutElement.preferredHeight = value;
                else
                    RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, value);
            }
        }

        public static Transform CreateSpacer(Transform parent, float width = 0f, float height = 0f)
        {
            var gameObject = new GameObject("Spacer")
            {
                layer = UiLayer
            };
            gameObject.transform.SetParent(parent, false);
            var layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            layoutElement.preferredWidth = width;
            return gameObject.transform;
        }

        public void InsertParentMargin(float width, float height) => CreateSpacer(Transform.parent, width, height);
        public void InsertMargin(float width, float height) => CreateSpacer(Transform, width, height);
        
        public LayoutContainer AddLayoutGroup<T>(string name,
            ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.Unconstrained,
            Vector2? pivotPoint = null, int padding = 0, TextAnchor childAlignment = TextAnchor.UpperLeft,
            bool expandChildWidth = false, bool expandChildHeight = false) where T : LayoutGroup
        {
            var gameObject = new GameObject(name)
            {
                layer = UiLayer
            };

            gameObject.transform.SetParent(Transform, false);
            
            var layoutGroup = gameObject.AddComponent<T>();
            layoutGroup.childAlignment = childAlignment;
            layoutGroup.padding = new RectOffset(padding, padding, padding, padding);

            var layoutType = layoutGroup.GetType();
            
            if (layoutType == typeof(StackLayoutGroup))
            {
                var stackLayoutGroup = (layoutGroup as StackLayoutGroup)!;
                stackLayoutGroup.childForceExpandWidth = expandChildWidth;
                stackLayoutGroup.childForceExpandHeight = expandChildHeight;
            }
            else if (layoutType == typeof(HorizontalOrVerticalLayoutGroup))
            {
                var horizontalOrVerticalLayoutGroup = (layoutGroup as HorizontalOrVerticalLayoutGroup)!;
                horizontalOrVerticalLayoutGroup.childForceExpandWidth = expandChildWidth;
                horizontalOrVerticalLayoutGroup.childForceExpandHeight = expandChildHeight;
            }


            var contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = verticalFit;
            contentSizeFitter.horizontalFit = horizontalFit;

            var rectTransform = (gameObject.transform as RectTransform)!;
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.sizeDelta = new Vector2(0, 0);

            var asContainer = new LayoutContainer(Builder, gameObject.transform);
            asContainer.RectTransform.pivot = pivotPoint ?? new Vector2(0.5f, 0.5f);
            return asContainer;
        }

        public LayoutContainer AddVerticalLayoutGroup(string name,
            ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.Unconstrained,
            Vector2? pivotPoint = null, int padding = 0, TextAnchor childAlignment = TextAnchor.UpperLeft,
            bool expandChildWidth = false, bool expandChildHeight = false)
            => AddLayoutGroup<VerticalLayoutGroup>(name, horizontalFit, verticalFit, pivotPoint, padding,
                childAlignment, expandChildWidth, expandChildHeight);

        public LayoutContainer AddHorizontalLayoutGroup(string name,
            ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.Unconstrained,
            Vector2? pivotPoint = null, int padding = 0, TextAnchor childAlignment = TextAnchor.UpperLeft,
            bool expandChildWidth = false, bool expandChildHeight = false)
            => AddLayoutGroup<HorizontalLayoutGroup>(name, horizontalFit, verticalFit, pivotPoint, padding,
                childAlignment, expandChildWidth, expandChildHeight);

        public void SetBackground(string backgroundType)
        {
            var bg = GameObject.GetOrAddComponent<Backgroundable>();
            bg.ApplyBackground(backgroundType);
        }

        public TkButton AddButton(string text, bool primary = false, int paddingHorizontal = 4, int paddingVertical = 2, 
            float preferredWidth = -1f, float preferredHeight = -1f, string? iconName = null, float iconSize = 10f,
            UnityAction? clickAction = null)
        {
            var bsmlButton = primary ? new PrimaryButtonTag() : new ButtonTag();
            var gameObject = bsmlButton.CreateObject(Transform);
            
            var wrapper = new TkButton(gameObject);
            wrapper.SetText(text);
            wrapper.SetPadding(paddingHorizontal, paddingVertical);
            wrapper.SetWidth(preferredWidth);
            wrapper.SetHeight(preferredHeight);
            if (iconName != null)
                _ = wrapper.SetIconAsync(iconName, iconSize, iconSize);
            if (clickAction != null)
                wrapper.AddClickHandler(clickAction);
            return wrapper;
        }

        public void AddHorizontalLine(float thickness = .25f, float width = -1f, Color? color = null)
        {
            var gameObject = new GameObject("HorizontalLine")
            {
                layer = UiLayer
            };
            gameObject.transform.SetParent(Transform, false);
            
            var layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = width;
            layoutElement.minHeight = thickness;
            layoutElement.preferredHeight = 0;
            layoutElement.flexibleHeight = 0;

            var image = gameObject.AddComponent<ImageView>();
            image.sprite = Utilities.ImageResources.WhitePixel;
            image.material = Utilities.ImageResources.NoGlowMat;
            image.color = color ?? BssbColors.VeryLightGray;
        }
    }
}