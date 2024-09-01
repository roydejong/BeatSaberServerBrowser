using System.Threading;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using HMUI;
using ServerBrowser.UI.Toolkit.Components;
using ServerBrowser.UI.Toolkit.Wrappers;
using ServerBrowser.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ServerBrowser.UI.Toolkit
{
    public class LayoutContainer : LayoutObject
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
            (layoutElement.transform as RectTransform)!.sizeDelta = new Vector2(width, height);
            return gameObject.transform;
        }

        public Transform InsertParentMargin(float width, float height) => CreateSpacer(Transform.parent, width, height);
        public Transform InsertMargin(float width, float height) => CreateSpacer(Transform, width, height);
        
        public LayoutContainer AddLayoutGroup<T>(string name,
            ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.Unconstrained,
            Vector2? pivotPoint = null, RectOffset? padding = null, TextAnchor childAlignment = TextAnchor.UpperLeft,
            bool expandChildWidth = false, bool expandChildHeight = false, bool isLayoutElement = true) where T : LayoutGroup
        {
            var gameObject = new GameObject(name)
            {
                layer = UiLayer
            };

            gameObject.transform.SetParent(Transform, false);
            
            var layoutGroup = gameObject.AddComponent<T>();
            layoutGroup.childAlignment = childAlignment;
            
            padding ??= new RectOffset(0, 0, 0, 0);
            layoutGroup.padding = padding;

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
            rectTransform.pivot = pivotPoint ?? new Vector2(0.5f, 0.5f);

            return new LayoutContainer(Builder, gameObject.transform, isLayoutElement);
        }

        public LayoutContainer AddVerticalLayoutGroup(string name,
            ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.Unconstrained,
            Vector2? pivotPoint = null, RectOffset? padding = null, TextAnchor childAlignment = TextAnchor.UpperLeft,
            bool expandChildWidth = false, bool expandChildHeight = false, bool isLayoutElement = true)
            => AddLayoutGroup<VerticalLayoutGroup>(name, horizontalFit, verticalFit, pivotPoint, padding,
                childAlignment, expandChildWidth, expandChildHeight, isLayoutElement);

        public LayoutContainer AddHorizontalLayoutGroup(string name,
            ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.Unconstrained,
            Vector2? pivotPoint = null, RectOffset? padding = null, TextAnchor childAlignment = TextAnchor.UpperLeft,
            bool expandChildWidth = false, bool expandChildHeight = false, bool isLayoutElement = true)
            => AddLayoutGroup<HorizontalLayoutGroup>(name, horizontalFit, verticalFit, pivotPoint, padding,
                childAlignment, expandChildWidth, expandChildHeight, isLayoutElement);

        public ImageView SetBackground(string backgroundType, bool noSkew = true, bool raycastTarget = true)
        {
            var bg = GameObject.GetOrAddComponent<Backgroundable>();
            bg.ApplyBackground(backgroundType);
            var imageView = bg.GetComponent<ImageView>();
            imageView.raycastTarget = raycastTarget;
            if (noSkew)
            {
                imageView.enabled = false;
                imageView._skew = 0f;
                imageView.enabled = true;
            }

            return imageView;
        }

        public void MakeButton(UnityAction clickAction)
        {
            var button = GameObject.GetOrAddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.AddListener(clickAction);
        }

        public TkButton AddButton(string text, bool primary = false, float preferredWidth = -1f,
            float preferredHeight = -1f, string? iconName = null, float iconSize = 10f,
            UnityAction? clickAction = null, bool noSkew = false, Color? highlightColor = null)
        {
            var bsmlButton = primary ? new PrimaryButtonTag() : new ButtonTag();
            var gameObject = bsmlButton.CreateObject(Transform);
            
            var button = new TkButton(gameObject);
            button.SetText(text);
            button.SetOuterPadding(4, 2);
            button.SetInternalPadding(3, 3, 0, 0);
            button.SetWidth(preferredWidth);
            button.SetHeight(preferredHeight);
            if (iconName != null)
                _ = button.SetIconAsync(iconName, iconSize, iconSize);
            if (clickAction != null)
                button.AddClickHandler(clickAction);
            if (noSkew)
                button.DisableSkew();
            if (highlightColor != null)
                button.SetHighlightColor(highlightColor.Value);
            return button;
        }

        public TkHorizontalSeparator AddHorizontalLine(float thickness = .25f, float width = -1f)
        {
            var line = Builder.CreateComponent<TkHorizontalSeparator>();
            line.AddToContainer(this);
            line.SetThickness(thickness);
            if (width > 0)
                line.SetPreferredWidth(width);
            return line;
        }
        
        public TkTextInputField AddTextInputField(string? placeholderText = "Search", string? iconSprite = null)
        {
            var searchInputField = Builder.CreateComponent<TkTextInputField>();
            searchInputField.AddToContainer(this);
            if (placeholderText != null)
                searchInputField.SetPlaceholderText(placeholderText);
            if (iconSprite != null)
                searchInputField.SetIconSprite(iconSprite);
            return searchInputField;
        }
        
        public TkFilterButton AddFilterButton(string? placeholderText = "No Filters")
        {
            var filterButton = Builder.CreateComponent<TkFilterButton>();
            filterButton.AddToContainer(this);
            if (placeholderText != null)
                filterButton.SetPlaceholderText(placeholderText);
            return filterButton;
        }

        public TkLoadingControl AddLoadingControl(float? preferredHeight = null)
        {
            var loadingControl = Builder.CreateComponent<TkLoadingControl>();
            loadingControl.AddToContainer(this);
            if (preferredHeight != null)
                loadingControl.SetPreferredHeight(preferredHeight.Value);
            return loadingControl;
        }

        public TkIcon AddIcon(string? spriteName, float? width = null, float? height = null)
        {
            var icon = Builder.CreateComponent<TkIcon>();
            icon.AddToContainer(this);
            if (spriteName != null)
                _ = icon.SetSprite(spriteName, CancellationToken.None);
            icon.SetPreferredSize(width, height);
            return icon;
        }
        
        public TkImageView AddAvatarImage(float? width = null, float? height = null)
        {
            var avatarImage = Builder.CreateComponent<TkImageView>();
            avatarImage.AddToContainer(this);
            avatarImage.SetPreferredSize(width, height);
            return avatarImage;
        }

        public TkText AddText(string text, Color? color = null, float? fontSize = null, float? width = null,
            float? height = null, TextAlignmentOptions? textAlignment = null, FontStyles? fontStyle = null)
        {
            var textComponent = Builder.CreateComponent<TkText>();
            textComponent.AddToContainer(this);
            textComponent.SetText(text);
            if (color != null)
                textComponent.SetTextColor(color.Value);
            if (fontSize != null)
                textComponent.SetFontSize(fontSize.Value);
            textComponent.SetPreferredSize(width, height);
            if (textAlignment != null)
                textComponent.SetTextAlignment(textAlignment.Value);
            if (fontStyle != null)
                textComponent.SetFontStyle(fontStyle.Value);
            return textComponent;
        }
        
        public TkScrollView AddScrollView()
        {
            var scrollView = Builder.CreateComponent<TkScrollView>();
            scrollView.AddToContainer(this);
            return scrollView;
        }

        public TkVerticalLayoutScrollView AddVerticalLayoutScrollView()
        {
            var scrollView = Builder.CreateComponent<TkVerticalLayoutScrollView>();
            scrollView.AddToContainer(this);
            return scrollView;
        }

        public TkServerCell AddServerCell()
        {
            var serverCell = Builder.CreateComponent<TkServerCell>();
            serverCell.AddToContainer(this);
            return serverCell;
        }

        public TkMasterServerCell AddMasterServerCell()
        {
            var serverCell = Builder.CreateComponent<TkMasterServerCell>();
            serverCell.AddToContainer(this);
            return serverCell;
        }
        
        public TkTableView AddTableView()
        {
            var tableView = Builder.CreateComponent<TkTableView>();
            tableView.AddToContainer(this);
            return tableView;
        }
        
        public TkAccountTile AddAccountTile()
        {
            var accountTile = Builder.CreateComponent<TkAccountTile>();
            accountTile.AddToContainer(this);
            return accountTile;
        }
        
        public TkToggleControl AddToggleControl(string? label = null, bool initialValue = false)
        {
            var toggle = Builder.CreateComponent<TkToggleControl>();
            toggle.AddToContainer(this);
            if (label != null)
                toggle.SetLabel(label);
            toggle.SetValue(initialValue);
            return toggle;
        }
        
        public TkDropdownControl AddDropdownControl(string? label = null)
        {
            var dropdown = Builder.CreateComponent<TkDropdownControl>();
            dropdown.AddToContainer(this);
            if (label != null)
                dropdown.SetLabel(label);
            return dropdown;
        }
    }
}