using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DspProgressionStatusExporter
{
    /// <summary>
    /// Independent read-only guide panel. It borrows serialized visual assets
    /// and anchoring from DSP's UIGoalPanel, but never registers native goals.
    /// </summary>
    internal sealed class GuidePanelController
    {
        private const float FallbackWidth = 440f;
        private const float OuterPadding = 12f;
        private const float SectionHeight = 25f;
        private const float FooterHeight = 42f;
        private const float RowGap = 5f;
        private const float PanelTop = 8f;
        private const float PanelRight = 28f;
        private const float SafeBottom = 230f;
        private const float ScrollControlWidth = 28f;
        private const float ScrollStep = 120f;
        private const float CubeRateSquareSize = 44f;
        private const float CubeRateGap = 4f;
        private const float CubeRateColumnInset =
            CubeRateSquareSize + CubeRateGap;
        private const float BodyRightInset =
            ScrollControlWidth + CubeRateColumnInset;
        private const float InteractiveControlHoverScale = 1.12f;
        private const float CompletionSeconds = 0.28f;
        private const float SnapshotFeedbackSeconds = 2f;
        private const string SourceGuideUrl =
            "https://dsp-beginner-guide.pages.dev/#";
        private const string DontPanicLabel = "DON'T\nPANIC";
        private static readonly Color DontPanicColor =
            new Color(1f, 0.04f, 0.07f, 1f);
        private static readonly Color SnapshotSuccessColor =
            new Color(0.23f, 1f, 0.44f, 1f);
        private static readonly Color SnapshotFailureColor =
            new Color(1f, 0.22f, 0.18f, 1f);
        private static readonly Color TextOutlineColor =
            new Color(0f, 0f, 0f, 0.9f);
        private static readonly Color SelectedControlOutlineColor =
            new Color(0.08f, 0.72f, 0.94f, 1f);

        private sealed class RowView
        {
            public GameObject Root;
            public Image Dot;
            public Image Check;
            public Text Label;
            public Text Detail;
            public readonly List<Image> Strikes = new List<Image>();
            public bool Completed;
            public float CompletionProgress;
            public float StrikeWidth;
        }

        private sealed class CubeRateView
        {
            public GameObject Root;
            public Image Background;
            public Text Rate;
            public string CubeId;
        }

        private sealed class ImageStyle
        {
            public Sprite Sprite;
            public Material Material;
            public Color Color = Color.white;
            public Image.Type Type = Image.Type.Simple;
            public bool PreserveAspect;

            public void Apply(Image image)
            {
                if (image == null) return;
                image.sprite = Sprite;
                image.material = Material;
                image.color = Color;
                image.type = Type;
                image.preserveAspect = PreserveAspect;
            }

            public static ImageStyle Capture(Image image)
            {
                if (image == null) return null;
                return new ImageStyle {
                    Sprite = image.sprite,
                    Material = image.material,
                    Color = image.color,
                    Type = image.type,
                    PreserveAspect = image.preserveAspect
                };
            }
        }

        private sealed class TextStyle
        {
            public Font Font;
            public int FontSize;
            public FontStyle FontStyle;
            public Color Color;
            public float LineSpacing;
            public Material Material;

            public void Apply(Text text)
            {
                if (text == null) return;
                text.font = Font;
                text.fontSize = FontSize;
                text.fontStyle = FontStyle;
                text.color = Color;
                text.lineSpacing = LineSpacing;
                text.material = Material;
                text.supportRichText = true;
            }

            public static TextStyle Capture(Text text, TextStyle fallback)
            {
                if (text == null)
                    return new TextStyle {
                        Font = fallback.Font,
                        FontSize = fallback.FontSize,
                        FontStyle = fallback.FontStyle,
                        Color = fallback.Color,
                        LineSpacing = fallback.LineSpacing,
                        Material = fallback.Material
                    };
                return new TextStyle {
                    Font = text.font,
                    FontSize = text.fontSize,
                    FontStyle = text.fontStyle,
                    Color = text.color,
                    LineSpacing = text.lineSpacing,
                    Material = text.material
                };
            }
        }

        private sealed class NativeGoalStyle
        {
            public Transform Parent;
            public RectTransform NativeRect;
            public TextStyle HeaderText;
            public TextStyle GroupText;
            public TextStyle InfoText;
            public Color CompletedTextColor;
            public ImageStyle Background;
            public ImageStyle Edge;
            public ImageStyle Dot;
            public ImageStyle Check;
            public ImageStyle Strike;
            public ImageStyle Collapse;
            public Vector2 CollapseSize = new Vector2(30f, 30f);

            public static NativeGoalStyle Capture(Font fallbackFont)
            {
                var fallbackHeader = new TextStyle {
                    Font = fallbackFont,
                    FontSize = 18,
                    FontStyle = FontStyle.Normal,
                    Color = new Color(0.92f, 0.94f, 0.95f, 1f),
                    LineSpacing = 1f
                };
                var fallbackInfo = new TextStyle {
                    Font = fallbackFont,
                    FontSize = 14,
                    FontStyle = FontStyle.Normal,
                    Color = new Color(0.87f, 0.89f, 0.9f, 1f),
                    LineSpacing = 1f
                };
                var style = new NativeGoalStyle {
                    HeaderText = fallbackHeader,
                    GroupText = fallbackInfo,
                    InfoText = fallbackInfo,
                    CompletedTextColor = new Color(0.68f, 0.72f, 0.73f, 1f),
                    Background = new ImageStyle {
                        Color = new Color(0.025f, 0.035f, 0.055f, 0.86f)
                    },
                    Edge = null,
                    Dot = new ImageStyle {
                        Color = new Color(0.8f, 0.82f, 0.83f, 1f)
                    },
                    Check = new ImageStyle {
                        Color = new Color(0.68f, 0.8f, 0.7f, 1f)
                    },
                    Strike = new ImageStyle {
                        Color = new Color(0.62f, 0.66f, 0.67f, 0.8f)
                    }
                };

                try
                {
                    Type rootType = FindType("UIRoot");
                    object root = GetStatic(rootType, "instance", "_instance");
                    object uiGame = GetMember(root, "uiGame");
                    object goalPanel = GetMember(uiGame, "goalPanel");
                    RectTransform nativeRect =
                        GetMember(goalPanel, "rect") as RectTransform;
                    if (nativeRect == null || nativeRect.parent == null)
                        return style;

                    Canvas nativeCanvas = nativeRect.GetComponentInParent<Canvas>();
                    style.Parent = nativeCanvas != null
                        ? nativeCanvas.transform
                        : nativeRect.parent;
                    style.NativeRect = nativeRect;
                    Text header = GetMember(goalPanel, "headerText") as Text;
                    style.HeaderText = TextStyle.Capture(header, fallbackHeader);
                    object normalHeaderColor =
                        GetMember(goalPanel, "normalHeaderTextColor");
                    if (normalHeaderColor is Color)
                        style.HeaderText.Color = (Color)normalHeaderColor;

                    object groupPrefab =
                        GetMember(goalPanel, "uiGoalGroupEntryPrefab");
                    Text groupText = GetMember(groupPrefab, "nameText") as Text;
                    style.GroupText = TextStyle.Capture(
                        groupText, style.HeaderText);
                    object groupColor =
                        GetMember(goalPanel, "normalHeaderOffSignTextColor");
                    if (groupColor is Color)
                        style.GroupText.Color = (Color)groupColor;

                    object infoPrefab =
                        GetMember(goalPanel, "uiGoalInfoEntryPrefab");
                    Text infoText = GetMember(infoPrefab, "nameText") as Text;
                    style.InfoText = TextStyle.Capture(infoText, fallbackInfo);
                    object normalInfoColor =
                        GetMember(goalPanel, "normalInfoTextColor");
                    if (normalInfoColor is Color)
                        style.InfoText.Color = (Color)normalInfoColor;
                    style.Dot = ImageStyle.Capture(
                        FindImage(GetMember(infoPrefab, "dotImage"))) ?? style.Dot;
                    style.Check = ImageStyle.Capture(
                        FindImage(GetMember(infoPrefab, "checkImage"))) ?? style.Check;
                    style.Strike = ImageStyle.Capture(
                        FindImage(GetMember(infoPrefab, "delLinePrefab"))) ??
                        ImageStyle.Capture(
                            FindImage(GetMember(goalPanel, "delLine"))) ??
                        style.Strike;

                    style.Background = ImageStyle.Capture(
                        FindImage(GetMember(goalPanel, "raycastBG"))) ??
                        style.Background;
                    style.Edge = ImageStyle.Capture(
                        FindImage(GetMember(goalPanel, "raycastBGEdge")));
                    Image collapse = FindImage(
                        GetMember(goalPanel, "collapseExpandImage"));
                    style.Collapse = ImageStyle.Capture(collapse);
                    if (collapse != null)
                    {
                        Vector2 capturedSize = collapse.rectTransform.rect.size;
                        style.CollapseSize = new Vector2(
                            capturedSize.x >= 12f && capturedSize.x <= 48f
                                ? capturedSize.x : 24f,
                            capturedSize.y >= 12f && capturedSize.y <= 48f
                                ? capturedSize.y : 24f);
                    }

                    object completedColor =
                        GetMember(goalPanel, "tickTextColor");
                    if (completedColor is Color)
                        style.CompletedTextColor = (Color)completedColor;
                }
                catch
                {
                    // A future DSP UI rename falls back to the safe local style.
                }
                return style;
            }

            private static Type FindType(string name)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type type = assembly.GetType(name, false);
                    if (type != null) return type;
                }
                return null;
            }

            private static object GetStatic(Type type, params string[] names)
            {
                if (type == null) return null;
                const BindingFlags flags =
                    BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic;
                foreach (string name in names)
                {
                    FieldInfo field = type.GetField(name, flags);
                    if (field != null) return field.GetValue(null);
                    PropertyInfo property = type.GetProperty(name, flags);
                    if (property != null && property.GetIndexParameters().Length == 0)
                        return property.GetValue(null, null);
                }
                return null;
            }

            private static object GetMember(object target, params string[] names)
            {
                if (target == null) return null;
                Type type = target.GetType();
                const BindingFlags flags =
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic;
                foreach (string name in names)
                {
                    FieldInfo field = type.GetField(name, flags);
                    if (field != null) return field.GetValue(target);
                    PropertyInfo property = type.GetProperty(name, flags);
                    if (property != null && property.GetIndexParameters().Length == 0)
                        return property.GetValue(target, null);
                }
                return null;
            }

            private static Image FindImage(object value)
            {
                if (value is Image) return (Image)value;
                Component component = value as Component;
                if (component != null) return component.GetComponent<Image>();
                GameObject gameObject = value as GameObject;
                return gameObject != null ? gameObject.GetComponent<Image>() : null;
            }
        }

        private GameObject fallbackCanvasObject;
        private GameObject panelObject;
        private RectTransform panelRect;
        private RectTransform parentRect;
        private RectTransform viewportRect;
        private RectTransform contentRect;
        private ScrollRect scrollRect;
        private Text titleText;
        private Text objectiveHeader;
        private Text pendingHeader;
        private Text contextHeader;
        private Text snapshotLinkText;
        private Text sourceGuideLinkText;
        private Font dontPanicFont;
        private bool ownsDontPanicFont;
        private EmbeddedBasicFont presentationFont;
        private RectTransform snapshotLinkRect;
        private RectTransform sourceGuideLinkRect;
        private RectTransform scrollUpRect;
        private RectTransform scrollDownRect;
        private RectTransform collapseButtonRect;
        private RectTransform previousPhaseRect;
        private RectTransform nextPhaseRect;
        private RectTransform cubeRateColumnRect;
        private Image collapseImage;
        private Text collapseFallbackText;
        private NativeGoalStyle style;
        private bool collapsed;
        private string phaseId;
        private Func<bool> snapshotAction;
        private Action<string> navigationAction;
        private bool bodyCanScroll;
        private float panelWidth;
        private Color snapshotLinkDefaultColor;
        private float snapshotFeedbackRemaining;
        private readonly List<RowView> objectiveViews = new List<RowView>();
        private readonly List<RowView> pendingViews = new List<RowView>();
        private readonly List<RowView> contextViews = new List<RowView>();
        private readonly List<CubeRateView> cubeRateViews =
            new List<CubeRateView>();

        public bool IsVisible
        {
            get { return panelObject != null && panelObject.activeSelf; }
        }

        public void Prepare()
        {
            EnsureCreated();
        }

        public void SetSnapshotAction(Func<bool> action)
        {
            snapshotAction = action;
        }

        public void SetNavigationAction(Action<string> action)
        {
            navigationAction = action;
        }

        public Dictionary<string, object> ExportDiagnostics()
        {
            EnsureCreated();
            var result = new Dictionary<string, object>();
            result["styleSource"] =
                style != null && style.NativeRect != null
                    ? "native-uigoalpanel"
                    : "fallback";
            result["parentName"] =
                panelRect != null && panelRect.parent != null
                    ? panelRect.parent.name
                    : null;
            result["adoptedWidth"] = panelWidth;
            result["anchorStrategy"] = "canvas-top-right-fixed";
            result["panelTop"] = PanelTop;
            result["panelRight"] = PanelRight;
            result["safeBottom"] = SafeBottom;
            result["boundedViewport"] = true;
            result["manualWheelSupport"] = false;
            result["scrollControls"] = "explicit-up-down-buttons";
            result["phaseNavigation"] =
                "player-controlled-critical-path-previous-next";
            result["panelPointerPolicy"] =
                "click-through-except-interactive-controls";
            result["cubeRateColumn"] =
                "native-one-minute-rates-click-through-fixed-below-collapse";
            result["textOutline"] = true;
            result["presentationFontSource"] =
                presentationFont != null
                    ? presentationFont.Source
                    : "not-loaded";
            result["phaseControlStyle"] =
                "transparent-bounded-hover-with-selected-outline";
            result["headerFont"] =
                style != null && style.HeaderText != null &&
                style.HeaderText.Font != null
                    ? style.HeaderText.Font.name
                    : null;
            result["headerFontSize"] =
                style != null && style.HeaderText != null
                    ? style.HeaderText.FontSize
                    : 0;
            result["infoFont"] =
                style != null && style.InfoText != null &&
                style.InfoText.Font != null
                    ? style.InfoText.Font.name
                    : null;
            result["infoFontSize"] =
                style != null && style.InfoText != null
                    ? style.InfoText.FontSize
                    : 0;
            result["sourceGuideLabel"] = "DON'T PANIC";
            result["sourceGuideFont"] =
                dontPanicFont != null ? dontPanicFont.name : null;
            result["sourceGuideFontSize"] =
                sourceGuideLinkText != null
                    ? sourceGuideLinkText.fontSize
                    : 0;
            result["sourceGuideColor"] =
                new Dictionary<string, object> {
                    { "r", DontPanicColor.r },
                    { "g", DontPanicColor.g },
                    { "b", DontPanicColor.b },
                    { "a", DontPanicColor.a }
                };
            result["nativeSprites"] = new Dictionary<string, object> {
                { "background", HasSprite(style != null ? style.Background : null) },
                { "edge", HasSprite(style != null ? style.Edge : null) },
                { "dot", HasSprite(style != null ? style.Dot : null) },
                { "check", HasSprite(style != null ? style.Check : null) },
                { "strike", HasSprite(style != null ? style.Strike : null) },
                { "collapse", HasSprite(style != null ? style.Collapse : null) }
            };
            if (style != null && style.NativeRect != null)
            {
                result["nativeAnchorMin"] = Vector(style.NativeRect.anchorMin);
                result["nativeAnchorMax"] = Vector(style.NativeRect.anchorMax);
                result["nativePivot"] = Vector(style.NativeRect.pivot);
                result["nativeAnchoredPosition"] =
                    Vector(style.NativeRect.anchoredPosition);
                result["nativeSize"] = Vector(style.NativeRect.rect.size);
            }
            return result;
        }

        public void Show(GuidePanelModel model)
        {
            EnsureCreated();
            panelObject.SetActive(true);
            Apply(model);
        }

        public void Hide()
        {
            ResetSnapshotFeedback();
            ResetInteractiveControlScales();
            if (panelObject != null) panelObject.SetActive(false);
        }

        public void UpdateModel(GuidePanelModel model)
        {
            if (!IsVisible || model == null) return;
            Apply(model);
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (!IsVisible) return;
            bool changed = AnimateRows(
                objectiveViews, unscaledDeltaTime);
            changed |= AnimateRows(
                pendingViews, unscaledDeltaTime);
            changed |= AnimateRows(
                contextViews, unscaledDeltaTime);
            if (snapshotFeedbackRemaining > 0f)
            {
                snapshotFeedbackRemaining = Mathf.Max(
                    0f, snapshotFeedbackRemaining - unscaledDeltaTime);
                if (snapshotFeedbackRemaining <= 0f)
                {
                    ResetSnapshotFeedback();
                    changed = true;
                }
            }

            if (changed) Canvas.ForceUpdateCanvases();
        }

        public void Destroy()
        {
            if (panelObject != null)
                UnityEngine.Object.Destroy(panelObject);
            if (fallbackCanvasObject != null)
                UnityEngine.Object.Destroy(fallbackCanvasObject);
            if (ownsDontPanicFont && dontPanicFont != null)
                UnityEngine.Object.Destroy(dontPanicFont);
            panelObject = null;
            fallbackCanvasObject = null;
            dontPanicFont = null;
            ownsDontPanicFont = false;
            if (presentationFont != null)
                presentationFont.Dispose();
            presentationFont = null;
            objectiveViews.Clear();
            pendingViews.Clear();
            contextViews.Clear();
            cubeRateViews.Clear();
        }

        private void EnsureCreated()
        {
            if (panelObject != null) return;
            objectiveViews.Clear();
            pendingViews.Clear();
            contextViews.Clear();
            cubeRateViews.Clear();
            phaseId = null;
            Font fallbackFont = FindFont();
            style = NativeGoalStyle.Capture(fallbackFont);
            presentationFont = EmbeddedBasicFont.Load(
                style.InfoText.Font,
                style.InfoText.FontSize);
            style.HeaderText.Font = presentationFont.Font;
            style.GroupText.Font = presentationFont.Font;
            style.InfoText.Font = presentationFont.Font;
            Transform parent = style.Parent;
            if (parent == null)
            {
                fallbackCanvasObject = new GameObject(
                    "DSPGuideCheckFallbackCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                UnityEngine.Object.DontDestroyOnLoad(fallbackCanvasObject);
                Canvas canvas = fallbackCanvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 32000;
                CanvasScaler scaler =
                    fallbackCanvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode =
                    CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode =
                    CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                parent = fallbackCanvasObject.transform;
            }

            parentRect = parent as RectTransform;
            panelObject = CreateObject(
                "DSPGuideCheckPanel",
                parent,
                typeof(Image));
            panelRect = panelObject.GetComponent<RectTransform>();
            ConfigurePanelAnchor();
            Image background = panelObject.GetComponent<Image>();
            style.Background.Apply(background);
            background.raycastTarget = false;

            if (style.Edge != null)
            {
                GameObject edgeObject = CreateObject(
                    "NativeGoalEdge", panelObject.transform, typeof(Image));
                RectTransform edgeRect =
                    edgeObject.GetComponent<RectTransform>();
                Stretch(edgeRect, 0f, 0f, 0f, 0f);
                Image edge = edgeObject.GetComponent<Image>();
                style.Edge.Apply(edge);
                edge.raycastTarget = false;
            }

            titleText = CreateText(
                "Title", panelObject.transform, style.HeaderText);
            titleText.alignment = TextAnchor.UpperLeft;

            GameObject collapseButton = CreateObject(
                "CollapseButton",
                panelObject.transform,
                typeof(Image),
                typeof(Button));
            collapseButtonRect =
                collapseButton.GetComponent<RectTransform>();
            Image buttonImage = collapseButton.GetComponent<Image>();
            buttonImage.color = new Color(1f, 1f, 1f, 0.001f);
            Button button = collapseButton.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            button.transition = Selectable.Transition.None;
            DisableNavigation(button);
            AddBoundedHoverScale(collapseButton, collapseButtonRect);
            button.onClick.AddListener(delegate {
                collapseButtonRect.localScale = Vector3.one;
                ToggleCollapsed();
            });
            if (style.Collapse != null && style.Collapse.Sprite != null)
            {
                GameObject iconObject = CreateObject(
                    "NativeCollapseIcon",
                    collapseButton.transform,
                    typeof(Image));
                collapseImage = iconObject.GetComponent<Image>();
                style.Collapse.Apply(collapseImage);
                collapseImage.raycastTarget = false;
                RectTransform iconRect = collapseImage.rectTransform;
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = style.CollapseSize;
                iconRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                collapseFallbackText = CreateText(
                    "CollapseGlyph", collapseButton.transform, style.HeaderText);
                collapseFallbackText.alignment = TextAnchor.MiddleCenter;
                Stretch(
                    collapseFallbackText.rectTransform, 0f, 0f, 0f, 0f);
            }

            GameObject cubeRateColumn = CreateObject(
                "CubeRateColumn", panelObject.transform);
            cubeRateColumnRect =
                cubeRateColumn.GetComponent<RectTransform>();

            CreateHeaderControl(
                "PreviousPhase",
                "\u25C0",
                delegate { Navigate("previous"); },
                out previousPhaseRect,
                out _);
            CreateHeaderControl(
                "NextPhase",
                "\u25B6",
                delegate { Navigate("next"); },
                out nextPhaseRect,
                out _);
            GameObject scrollObject = CreateObject(
                "ScrollArea",
                panelObject.transform,
                typeof(ScrollRect));
            scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = false;
            scrollRect.scrollSensitivity = 0f;

            GameObject viewportObject = CreateObject(
                "Viewport",
                scrollObject.transform,
                typeof(Image),
                typeof(RectMask2D));
            viewportRect = viewportObject.GetComponent<RectTransform>();
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = false;
            Stretch(viewportRect, 0f, 0f, 0f, 0f);

            GameObject contentObject = CreateObject(
                "Content", viewportObject.transform);
            contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            objectiveHeader = CreateText(
                "ObjectivesHeader", contentObject.transform, style.GroupText);
            objectiveHeader.text = "Objectives";
            pendingHeader = CreateText(
                "PendingHeader", contentObject.transform, style.GroupText);
            pendingHeader.text = "Pending:";
            contextHeader = CreateText(
                "ContextHeader", contentObject.transform, style.GroupText);
            contextHeader.text = "Current Status";

            snapshotLinkText = CreateFooterLink(
                "SnapshotLink",
                panelObject.transform,
                "Save snapshot",
                SaveSnapshot,
                out snapshotLinkRect);
            snapshotLinkDefaultColor = snapshotLinkText.color;
            sourceGuideLinkText = CreateFooterLink(
                "SourceGuideLink",
                panelObject.transform,
                DontPanicLabel,
                OpenSourceGuide,
                out sourceGuideLinkRect);
            dontPanicFont = FindDontPanicFont(
                style.InfoText.Font,
                style.InfoText.FontSize + 3,
                out ownsDontPanicFont);
            sourceGuideLinkText.font = dontPanicFont;
            sourceGuideLinkText.fontSize =
                style.InfoText.FontSize + 3;
            sourceGuideLinkText.fontStyle = FontStyle.Bold;
            sourceGuideLinkText.color = DontPanicColor;
            sourceGuideLinkText.lineSpacing = 0.82f;
            sourceGuideLinkText.alignment = TextAnchor.MiddleCenter;
            CreateScrollControl(
                "ScrollUp",
                "▲",
                ScrollUp,
                out scrollUpRect);
            CreateScrollControl(
                "ScrollDown",
                "▼",
                ScrollDown,
                out scrollDownRect);
            cubeRateColumnRect.SetAsLastSibling();

            panelObject.transform.SetAsLastSibling();
            panelObject.SetActive(false);
        }

        private void ConfigurePanelAnchor()
        {
            RectTransform native = style.NativeRect;
            if (native != null)
            {
                panelWidth = native.rect.width;
                if (panelWidth < 280f || panelWidth > 720f)
                    panelWidth = Mathf.Abs(native.sizeDelta.x);
            }
            if (panelWidth < 280f || panelWidth > 720f)
                panelWidth = FallbackWidth;
            ResetPanelAnchor();
            panelRect.sizeDelta = new Vector2(panelWidth, 300f);
        }

        private void ResetPanelAnchor()
        {
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition =
                new Vector2(-PanelRight, -PanelTop);
            panelRect.localRotation = Quaternion.identity;
            panelRect.localScale = Vector3.one;
        }

        private void Apply(GuidePanelModel model)
        {
            if (model == null) return;
            titleText.text = GuideRichText.Title(model.PhaseId, model.Title);
            snapshotLinkText.text = "Save snapshot";
            sourceGuideLinkText.text = DontPanicLabel;

            bool phaseChanged = !String.Equals(
                phaseId, model.PhaseId, StringComparison.OrdinalIgnoreCase);
            if (phaseChanged || !RowsMatch(objectiveViews, model.Objectives))
            {
                phaseId = model.PhaseId;
                RebuildRows(objectiveViews, model.Objectives, "Objective");
            }
            else
                UpdateRows(objectiveViews, model.Objectives);

            if (!RowsMatch(pendingViews, model.Pending))
                RebuildRows(pendingViews, model.Pending, "Pending");
            else
                UpdateRows(pendingViews, model.Pending);

            if (!RowsMatch(contextViews, model.Context))
                RebuildRows(contextViews, model.Context, "Context");
            else
                UpdateRows(contextViews, model.Context);

            ApplyCubeRates(model.CubeRates);

            Layout();
            if (phaseChanged)
                contentRect.anchoredPosition = Vector2.zero;
        }

        private void ApplyCubeRates(List<GuidePanelCubeRateModel> rates)
        {
            bool rebuild = cubeRateViews.Count != rates.Count;
            if (!rebuild)
                for (int i = 0; i < rates.Count; i++)
                    if (!String.Equals(
                        cubeRateViews[i].CubeId,
                        rates[i].CubeId,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        rebuild = true;
                        break;
                    }

            if (rebuild)
            {
                foreach (CubeRateView view in cubeRateViews)
                    if (view.Root != null)
                        UnityEngine.Object.Destroy(view.Root);
                cubeRateViews.Clear();
                foreach (GuidePanelCubeRateModel rate in rates)
                {
                    GameObject root = CreateObject(
                        "CubeRate-" + rate.CubeId,
                        cubeRateColumnRect,
                        typeof(Image));
                    Image background = root.GetComponent<Image>();
                    background.raycastTarget = false;
                    Text text = CreateText(
                        "Rate", root.transform, style.InfoText);
                    text.alignment = TextAnchor.MiddleCenter;
                    text.fontSize = Math.Max(10, style.InfoText.FontSize - 1);
                    Stretch(text.rectTransform, 2f, 2f, 2f, 2f);
                    cubeRateViews.Add(new CubeRateView {
                        Root = root,
                        Background = background,
                        Rate = text,
                        CubeId = rate.CubeId
                    });
                }
            }

            for (int i = 0; i < rates.Count; i++)
            {
                GuidePanelCubeRateModel rate = rates[i];
                CubeRateView view = cubeRateViews[i];
                view.Background.color = CubeBackgroundColor(rate.CubeId);
                view.Rate.text = rate.RateText;
                view.Rate.color = CubeRateTextColor(rate.Level);
            }
        }

        private static Color CubeBackgroundColor(string cubeId)
        {
            if (String.Equals(cubeId, "blue", StringComparison.OrdinalIgnoreCase))
                return new Color(0.05f, 0.62f, 1f, 0.38f);
            if (String.Equals(cubeId, "red", StringComparison.OrdinalIgnoreCase))
                return new Color(1f, 0.12f, 0.16f, 0.38f);
            if (String.Equals(cubeId, "yellow", StringComparison.OrdinalIgnoreCase))
                return new Color(1f, 0.72f, 0.04f, 0.38f);
            if (String.Equals(cubeId, "purple", StringComparison.OrdinalIgnoreCase))
                return new Color(0.68f, 0.18f, 1f, 0.38f);
            if (String.Equals(cubeId, "green", StringComparison.OrdinalIgnoreCase))
                return new Color(0.12f, 0.9f, 0.32f, 0.38f);
            return new Color(0.9f, 0.94f, 1f, 0.38f);
        }

        private static Color CubeRateTextColor(CubeRateLevel level)
        {
            if (level == CubeRateLevel.BelowMinimum)
                return new Color(1f, 0.12f, 0.12f, 1f);
            if (level == CubeRateLevel.Minimum)
                return new Color(1f, 0.55f, 0.08f, 1f);
            if (level == CubeRateLevel.Later)
                return new Color(0.22f, 1f, 0.4f, 1f);
            return Color.white;
        }

        private bool RowsMatch(
            List<RowView> views,
            List<GuidePanelRowModel> rows)
        {
            if (views.Count != rows.Count) return false;
            for (int i = 0; i < rows.Count; i++)
                if (!String.Equals(
                    views[i].Root.name,
                    "GuideRow-" + rows[i].Id,
                    StringComparison.Ordinal))
                    return false;
            return true;
        }

        private void RebuildRows(
            List<RowView> views,
            List<GuidePanelRowModel> rows,
            string group)
        {
            foreach (RowView view in views)
                if (view.Root != null)
                    UnityEngine.Object.Destroy(view.Root);
            views.Clear();
            foreach (GuidePanelRowModel row in rows)
                views.Add(CreateRow(group, row));
        }

        private RowView CreateRow(string group, GuidePanelRowModel row)
        {
            GameObject root = CreateObject(
                "GuideRow-" + row.Id, contentRect);
            Image background = root.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0f);
            background.raycastTarget = false;

            GameObject dotObject = CreateObject(
                "Dot", root.transform, typeof(Image));
            Image dot = dotObject.GetComponent<Image>();
            style.Dot.Apply(dot);
            dot.raycastTarget = false;

            GameObject checkObject = CreateObject(
                "Check", root.transform, typeof(Image));
            Image check = checkObject.GetComponent<Image>();
            style.Check.Apply(check);
            check.raycastTarget = false;

            Text label = CreateText(
                "Label", root.transform, style.InfoText);
            Text detail = CreateText(
                "Detail", root.transform, style.InfoText);
            Color detailColor = detail.color;
            detailColor.a *= 0.75f;
            detail.color = detailColor;

            RowView view = new RowView {
                Root = root,
                Dot = dot,
                Check = check,
                Label = label,
                Detail = detail
            };
            ApplyRow(view, row);
            return view;
        }

        private void UpdateRows(
            List<RowView> views,
            List<GuidePanelRowModel> rows)
        {
            for (int i = 0; i < rows.Count; i++)
                ApplyRow(views[i], rows[i]);
        }

        private void ApplyRow(RowView view, GuidePanelRowModel row)
        {
            bool wasCompleted = view.Completed;
            view.Completed = row.Completed;
            if (view.Completed && !wasCompleted)
                view.CompletionProgress = 0f;
            else if (!view.Completed)
                view.CompletionProgress = 0f;

            view.Dot.enabled = false;
            view.Label.text = GuideRichText.Cubes(row.Label);
            view.Detail.text = GuideRichText.Cubes(row.Detail ?? "");
            view.Label.color = row.Completed
                ? style.CompletedTextColor
                : style.InfoText.Color;
            Color detailColor = row.Completed
                ? style.CompletedTextColor
                : style.InfoText.Color;
            detailColor.a *= 0.72f;
            view.Detail.color = detailColor;
            UpdateCompletionVisual(view);
        }

        private void Layout()
        {
            ResetPanelAnchor();

            previousPhaseRect.gameObject.SetActive(!String.Equals(
                phaseId, "bootstrap", StringComparison.OrdinalIgnoreCase));
            nextPhaseRect.gameObject.SetActive(!String.Equals(
                phaseId, "white", StringComparison.OrdinalIgnoreCase));

            float controlsRight = panelWidth - 52f;
            SetTopRect(
                nextPhaseRect, controlsRight - 28f, 10f, 26f, 28f);
            controlsRight -= 31f;
            SetTopRect(
                previousPhaseRect, controlsRight - 28f, 10f, 26f, 28f);
            controlsRight -= 31f;

            float titleRight = panelWidth - controlsRight + 4f;
            float titleWidth = Mathf.Max(
                150f, controlsRight - OuterPadding - 4f);
            SetTopStretchRect(
                titleText.rectTransform,
                OuterPadding,
                9f,
                titleRight,
                78f);
            titleText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, titleWidth);
            float titleHeight = Mathf.Max(
                28f, Mathf.Min(78f, titleText.preferredHeight));
            titleText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical, titleHeight);
            float headerHeight = Mathf.Max(48f, titleHeight + 15f);
            SetTopRect(
                collapseButtonRect,
                panelWidth - 48f,
                7f,
                38f,
                38f);
            SetTopRect(
                cubeRateColumnRect,
                panelWidth - 51f,
                49f,
                CubeRateSquareSize,
                cubeRateViews.Count * (CubeRateSquareSize + CubeRateGap));
            for (int i = 0; i < cubeRateViews.Count; i++)
                SetTopRect(
                    cubeRateViews[i].Root.GetComponent<RectTransform>(),
                    0f,
                    i * (CubeRateSquareSize + CubeRateGap),
                    CubeRateSquareSize,
                    CubeRateSquareSize);
            if (collapseImage != null)
                collapseImage.rectTransform.localEulerAngles =
                    new Vector3(0f, 0f, collapsed ? 180f : 0f);
            if (collapseFallbackText != null)
                collapseFallbackText.text = collapsed ? "+" : "−";

            if (collapsed)
            {
                viewportRect.gameObject.SetActive(false);
                snapshotLinkRect.gameObject.SetActive(false);
                sourceGuideLinkRect.gameObject.SetActive(false);
                scrollUpRect.gameObject.SetActive(false);
                scrollDownRect.gameObject.SetActive(false);
                panelRect.sizeDelta = new Vector2(panelWidth, headerHeight);
                return;
            }

            viewportRect.gameObject.SetActive(true);
            snapshotLinkRect.gameObject.SetActive(true);
            sourceGuideLinkRect.gameObject.SetActive(true);

            float y = 2f;
            LayoutSection(
                objectiveHeader, objectiveViews, ref y, true);
            LayoutSection(
                pendingHeader, pendingViews, ref y, pendingViews.Count > 0);
            LayoutSection(
                contextHeader, contextViews, ref y, contextViews.Count > 0);
            float contentHeight = y + 5f;
            contentRect.sizeDelta = new Vector2(0f, contentHeight);

            float desiredHeight =
                headerHeight + contentHeight + FooterHeight + 8f;
            float maxHeight = AvailableHeight();
            float panelHeight = Mathf.Min(desiredHeight, maxHeight);
            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

            Stretch(
                scrollRect.GetComponent<RectTransform>(),
                0f,
                FooterHeight,
                BodyRightInset,
                headerHeight);
            SetBottomRect(
                snapshotLinkRect,
                OuterPadding,
                4f,
                118f,
                FooterHeight - 5f);
            SetBottomRect(
                sourceGuideLinkRect,
                panelWidth - OuterPadding - 98f,
                4f,
                98f,
                FooterHeight - 5f);
            SetTopRect(
                scrollUpRect,
                panelWidth - BodyRightInset + 2f,
                headerHeight + 4f,
                ScrollControlWidth - 4f,
                26f);
            SetBottomRect(
                scrollDownRect,
                panelWidth - BodyRightInset + 2f,
                FooterHeight + 4f,
                ScrollControlWidth - 4f,
                26f);

            Canvas.ForceUpdateCanvases();
            float viewportHeight =
                Mathf.Max(0f, panelHeight - headerHeight - FooterHeight);
            bool canScroll = contentHeight > viewportHeight + 1f;
            bodyCanScroll = canScroll;
            scrollUpRect.gameObject.SetActive(canScroll);
            scrollDownRect.gameObject.SetActive(canScroll);
            if (!canScroll)
            {
                contentRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                ClampScrollPosition();
            }
        }

        private void LayoutSection(
            Text header,
            List<RowView> rows,
            ref float y,
            bool visible)
        {
            header.gameObject.SetActive(visible);
            if (!visible) return;
            SetTopStretchRect(
                header.rectTransform,
                OuterPadding,
                y,
                OuterPadding,
                SectionHeight);
            y += SectionHeight;
            foreach (RowView view in rows)
            {
                float rowHeight = LayoutRow(view, y);
                y += rowHeight + RowGap;
            }
            y += 5f;
        }

        private float LayoutRow(RowView view, float top)
        {
            float iconSize = 18f;
            float textLeft = 29f;
            float textWidth =
                panelWidth - BodyRightInset -
                (OuterPadding * 2f) - textLeft;
            view.Label.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, textWidth);
            view.Detail.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, textWidth);

            float labelHeight = Mathf.Max(
                style.InfoText.FontSize + 5f,
                view.Label.preferredHeight);
            float detailHeight = String.IsNullOrEmpty(view.Detail.text)
                ? 0f
                : Mathf.Max(
                    style.InfoText.FontSize + 3f,
                    view.Detail.preferredHeight);
            float rowHeight = Mathf.Max(
                iconSize,
                labelHeight + (detailHeight > 0f ? detailHeight + 2f : 0f)) + 3f;

            RectTransform root = view.Root.GetComponent<RectTransform>();
            SetTopStretchRect(
                root, OuterPadding, top, OuterPadding, rowHeight);
            SetTopRect(view.Dot.rectTransform, 2f, 2f, iconSize, iconSize);
            SetTopRect(view.Check.rectTransform, 2f, 2f, iconSize, iconSize);
            SetTopStretchRect(
                view.Label.rectTransform,
                textLeft,
                0f,
                0f,
                labelHeight);
            SetTopStretchRect(
                view.Detail.rectTransform,
                textLeft,
                labelHeight + 1f,
                0f,
                detailHeight);

            int lineCount = Mathf.Max(
                1, view.Label.cachedTextGenerator.lineCount);
            EnsureStrikeLines(view, lineCount);
            view.StrikeWidth = textWidth;
            float lineHeight = labelHeight / lineCount;
            for (int i = 0; i < view.Strikes.Count; i++)
            {
                RectTransform strikeRect = view.Strikes[i].rectTransform;
                strikeRect.anchorMin = new Vector2(0f, 1f);
                strikeRect.anchorMax = new Vector2(0f, 1f);
                strikeRect.pivot = new Vector2(0f, 0.5f);
                strikeRect.anchoredPosition = new Vector2(
                    0f, -((i + 0.54f) * lineHeight));
                strikeRect.sizeDelta = new Vector2(textWidth, 1.5f);
            }
            UpdateCompletionVisual(view);
            return rowHeight;
        }

        private float AvailableHeight()
        {
            if (parentRect != null && parentRect.rect.height > 200f)
                return Mathf.Max(
                    260f,
                    parentRect.rect.height - PanelTop - SafeBottom);
            Canvas canvas = panelObject.GetComponentInParent<Canvas>();
            RectTransform canvasRect =
                canvas != null ? canvas.transform as RectTransform : null;
            return canvasRect != null && canvasRect.rect.height > 200f
                ? Mathf.Max(
                    260f,
                    canvasRect.rect.height - PanelTop - SafeBottom)
                : 820f;
        }

        private void EnsureStrikeLines(RowView view, int count)
        {
            while (view.Strikes.Count < count)
            {
                GameObject strikeObject = CreateObject(
                    "Strike-" + view.Strikes.Count,
                    view.Label.transform,
                    typeof(Image));
                Image strike = strikeObject.GetComponent<Image>();
                strike.sprite = null;
                strike.material = null;
                strike.type = Image.Type.Simple;
                strike.color = style.Strike != null
                    ? style.Strike.Color
                    : style.CompletedTextColor;
                strike.raycastTarget = false;
                view.Strikes.Add(strike);
            }
            while (view.Strikes.Count > count)
            {
                int last = view.Strikes.Count - 1;
                Image strike = view.Strikes[last];
                view.Strikes.RemoveAt(last);
                if (strike != null)
                    UnityEngine.Object.Destroy(strike.gameObject);
            }
        }

        private bool AnimateRows(
            List<RowView> views,
            float unscaledDeltaTime)
        {
            bool changed = false;
            foreach (RowView view in views)
            {
                if (!view.Completed ||
                    view.CompletionProgress >= 1f)
                    continue;
                view.CompletionProgress = Mathf.Min(
                    1f,
                    view.CompletionProgress +
                    (unscaledDeltaTime / CompletionSeconds));
                UpdateCompletionVisual(view);
                changed = true;
            }
            return changed;
        }

        private void UpdateCompletionVisual(RowView view)
        {
            float progress = view.Completed
                ? Mathf.SmoothStep(
                    0f, 1f, Mathf.Clamp01(view.CompletionProgress))
                : 0f;
            view.Check.enabled = view.Completed;
            if (view.Completed)
            {
                Color checkColor = style.Check != null
                    ? style.Check.Color
                    : Color.white;
                checkColor.a *= progress;
                view.Check.color = checkColor;
                view.Check.rectTransform.localScale =
                    Vector3.one * Mathf.Lerp(0.65f, 1f, progress);
            }

            int lineCount = Mathf.Max(1, view.Strikes.Count);
            for (int i = 0; i < view.Strikes.Count; i++)
            {
                Image strike = view.Strikes[i];
                strike.enabled = view.Completed;
                if (!view.Completed) continue;
                float lineProgress = Mathf.Clamp01(
                    (progress * lineCount) - i);
                RectTransform rect = strike.rectTransform;
                rect.sizeDelta = new Vector2(
                    view.StrikeWidth * lineProgress,
                    rect.sizeDelta.y);
            }
        }

        private Camera CanvasCamera()
        {
            Canvas canvas = panelObject != null
                ? panelObject.GetComponentInParent<Canvas>()
                : null;
            return canvas != null &&
                canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
        }

        private Text CreateFooterLink(
            string name,
            Transform parent,
            string label,
            UnityEngine.Events.UnityAction action,
            out RectTransform rect)
        {
            GameObject linkObject = CreateObject(
                name, parent, typeof(Image), typeof(Button));
            rect = linkObject.GetComponent<RectTransform>();
            Image hitArea = linkObject.GetComponent<Image>();
            hitArea.color = new Color(1f, 1f, 1f, 0.001f);
            Button button = linkObject.GetComponent<Button>();
            button.targetGraphic = hitArea;
            button.transition = Selectable.Transition.None;
            DisableNavigation(button);
            RectTransform linkRect = rect;
            AddBoundedHoverScale(linkObject, linkRect);
            button.onClick.AddListener(delegate {
                linkRect.localScale = Vector3.one;
                action();
            });

            Text text = CreateText(
                name + "Text", linkObject.transform, style.InfoText);
            text.text = label;
            text.alignment = TextAnchor.MiddleLeft;
            Color color = text.color;
            color.a *= 0.82f;
            text.color = color;
            Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
            return text;
        }

        private void CreateHeaderControl(
            string name,
            string label,
            UnityEngine.Events.UnityAction action,
            out RectTransform rect,
            out Outline outline)
        {
            GameObject control = CreateObject(
                name,
                panelObject.transform,
                typeof(Image),
                typeof(Button));
            rect = control.GetComponent<RectTransform>();
            Image hitArea = control.GetComponent<Image>();
            hitArea.color = new Color(1f, 1f, 1f, 0f);
            hitArea.raycastTarget = true;
            Button button = control.GetComponent<Button>();
            button.targetGraphic = hitArea;
            button.transition = Selectable.Transition.None;
            DisableNavigation(button);
            Text text = CreateText(
                name + "Text", control.transform, style.GroupText);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
            outline = text.GetComponent<Outline>();
            RectTransform controlRect = rect;
            AddBoundedHoverScale(control, controlRect);
            button.onClick.AddListener(delegate {
                controlRect.localScale = Vector3.one;
                action();
            });
        }

        private void CreateScrollControl(
            string name,
            string glyph,
            UnityEngine.Events.UnityAction action,
            out RectTransform rect)
        {
            GameObject control = CreateObject(
                name, panelObject.transform, typeof(Image), typeof(Button));
            rect = control.GetComponent<RectTransform>();
            Image background = control.GetComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0f);
            background.raycastTarget = true;
            Button button = control.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            DisableNavigation(button);
            RectTransform controlRect = rect;
            AddBoundedHoverScale(control, controlRect);
            button.onClick.AddListener(delegate {
                controlRect.localScale = Vector3.one;
                action();
            });
            Text text = CreateText(
                name + "Glyph", control.transform, style.GroupText);
            text.text = glyph;
            text.alignment = TextAnchor.MiddleCenter;
            Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
            control.SetActive(false);
        }

        private void SaveSnapshot()
        {
            bool succeeded = false;
            try
            {
                succeeded =
                    snapshotAction != null && snapshotAction();
            }
            catch
            {
                succeeded = false;
            }
            finally
            {
                snapshotFeedbackRemaining = SnapshotFeedbackSeconds;
                if (snapshotLinkText != null)
                    snapshotLinkText.color = succeeded
                        ? SnapshotSuccessColor
                        : SnapshotFailureColor;
                Canvas.ForceUpdateCanvases();
                ClearButtonFocus();
            }
        }

        private void ResetSnapshotFeedback()
        {
            snapshotFeedbackRemaining = 0f;
            if (snapshotLinkText != null)
                snapshotLinkText.color = snapshotLinkDefaultColor;
        }

        private void OpenSourceGuide()
        {
            string anchor = String.IsNullOrEmpty(phaseId)
                ? "top"
                : phaseId.ToLowerInvariant();
            Application.OpenURL(SourceGuideUrl + anchor);
            ClearButtonFocus();
        }

        private void ToggleCollapsed()
        {
            collapsed = !collapsed;
            Layout();
            ClearButtonFocus();
        }

        private void Navigate(string command)
        {
            try
            {
                if (navigationAction != null)
                    navigationAction(command);
            }
            finally
            {
                ClearButtonFocus();
            }
        }

        private void ScrollUp()
        {
            ScrollBody(-ScrollStep);
            ClearButtonFocus();
        }

        private void ScrollDown()
        {
            ScrollBody(ScrollStep);
            ClearButtonFocus();
        }

        private void ScrollBody(float delta)
        {
            if (!bodyCanScroll || contentRect == null || viewportRect == null)
                return;
            Vector2 position = contentRect.anchoredPosition;
            position.y += delta;
            contentRect.anchoredPosition = position;
            ClampScrollPosition();
        }

        private void ClampScrollPosition()
        {
            if (contentRect == null || viewportRect == null) return;
            float overflow = Mathf.Max(
                0f, contentRect.rect.height - viewportRect.rect.height);
            Vector2 position = contentRect.anchoredPosition;
            position.x = 0f;
            position.y = Mathf.Clamp(position.y, 0f, overflow);
            contentRect.anchoredPosition = position;
        }

        private static void ClearButtonFocus()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private static void DisableNavigation(Selectable selectable)
        {
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        private static void AddBoundedHoverScale(
            GameObject control,
            RectTransform rect)
        {
            EventTrigger trigger = control.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            AddPointerTrigger(
                trigger,
                EventTriggerType.PointerEnter,
                delegate {
                    rect.localScale =
                        Vector3.one * InteractiveControlHoverScale;
                });
            AddPointerTrigger(
                trigger,
                EventTriggerType.PointerExit,
                delegate { rect.localScale = Vector3.one; });
        }

        private static void AddPointerTrigger(
            EventTrigger trigger,
            EventTriggerType type,
            UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            var entry = new EventTrigger.Entry {
                eventID = type,
                callback = new EventTrigger.TriggerEvent()
            };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        private void ResetInteractiveControlScales()
        {
            ResetScale(collapseButtonRect);
            ResetScale(previousPhaseRect);
            ResetScale(nextPhaseRect);
            ResetScale(scrollUpRect);
            ResetScale(scrollDownRect);
            ResetScale(snapshotLinkRect);
            ResetScale(sourceGuideLinkRect);
        }

        private static void ResetScale(RectTransform rect)
        {
            if (rect != null) rect.localScale = Vector3.one;
        }

        private Text CreateText(
            string name,
            Transform parent,
            TextStyle textStyle)
        {
            GameObject child = CreateObject(name, parent, typeof(Text));
            Text text = child.GetComponent<Text>();
            textStyle.Apply(text);
            Outline outline = child.AddComponent<Outline>();
            outline.effectColor = TextOutlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = false;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateObject(
            string name,
            Transform parent,
            params Type[] components)
        {
            var types = new List<Type>();
            types.Add(typeof(RectTransform));
            if (components != null) types.AddRange(components);
            GameObject child = new GameObject(name, types.ToArray());
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void Stretch(
            RectTransform rect,
            float left,
            float bottom,
            float right,
            float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetTopRect(
            RectTransform rect,
            float left,
            float top,
            float width,
            float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetTopStretchRect(
            RectTransform rect,
            float left,
            float top,
            float right,
            float heightOrBottom)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -top - heightOrBottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetBottomStretchRect(
            RectTransform rect,
            float left,
            float bottom,
            float right,
            float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, bottom + height);
        }

        private static void SetBottomRect(
            RectTransform rect,
            float left,
            float bottom,
            float width,
            float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(left, bottom);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static Font FindFont()
        {
            Font[] loaded = Resources.FindObjectsOfTypeAll<Font>();
            if (loaded != null)
                foreach (Font candidate in loaded)
                    if (candidate != null &&
                        candidate.name.IndexOf(
                            "saira", StringComparison.OrdinalIgnoreCase) >= 0)
                        return candidate;
            if (loaded != null && loaded.Length > 0 && loaded[0] != null)
                return loaded[0];
            try { return Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            catch
            {
                return Font.CreateDynamicFontFromOSFont(
                    new string[] { "Arial", "Segoe UI" }, 16);
            }
        }

        private static Font FindDontPanicFont(
            Font fallback,
            int size,
            out bool ownsFont)
        {
            ownsFont = false;
            Font[] loaded = Resources.FindObjectsOfTypeAll<Font>();
            if (loaded != null)
                foreach (Font candidate in loaded)
                    if (candidate != null &&
                        candidate.name.IndexOf(
                            "comic sans",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return candidate;
            try
            {
                Font font = Font.CreateDynamicFontFromOSFont(
                    new string[] { "Comic Sans MS", "Comic Sans" },
                    size);
                if (font != null)
                {
                    ownsFont = true;
                    return font;
                }
            }
            catch
            {
                // The footer remains usable with the native fallback font.
            }
            return fallback;
        }

        private static bool HasSprite(ImageStyle imageStyle)
        {
            return imageStyle != null && imageStyle.Sprite != null;
        }

        private static Dictionary<string, object> Vector(Vector2 vector)
        {
            return new Dictionary<string, object> {
                { "x", vector.x },
                { "y", vector.y }
            };
        }

        private static class GuideRichText
        {
            private static readonly Dictionary<string, Color> PhaseColors =
                new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase) {
                    { "blue", new Color(0.22f, 0.72f, 1f) },
                    { "red", new Color(1f, 0.31f, 0.35f) },
                    { "yellow", new Color(1f, 0.76f, 0.18f) },
                    { "purple", new Color(0.72f, 0.42f, 1f) },
                    { "green", new Color(0.27f, 0.9f, 0.48f) },
                    { "white", new Color(0.94f, 0.94f, 0.96f) }
                };

            private static readonly string[] CubePhrases = new string[] {
                "Blue Cubes (Electromagnetic Matrices)",
                "Blue Cube (Electromagnetic Matrix)",
                "Red Cubes (Energy Matrices)",
                "Red Cube (Energy Matrix)",
                "Yellow Cubes (Structure Matrices)",
                "Yellow Cube (Structure Matrix)",
                "Purple Cubes (Information Matrices)",
                "Purple Cube (Information Matrix)",
                "Green Cubes (Gravity Matrices)",
                "Green Cube (Gravity Matrix)",
                "White Cubes (Universe Matrices)",
                "White Cube (Universe Matrix)"
            };

            public static string Title(string phaseId, string title)
            {
                Color color = PhaseColor(phaseId);
                return Colorize(
                    Escape((phaseId ?? "Guide").ToUpperInvariant()),
                    color) + "  —  " + Cubes(title);
            }

            public static string Cubes(string text)
            {
                string result = Escape(text ?? "");
                foreach (string phrase in CubePhrases)
                {
                    string escaped = Escape(phrase);
                    string colorName = phrase.Substring(
                        0, phrase.IndexOf(' '));
                    result = result.Replace(
                        escaped,
                        Colorize(escaped, PhaseColor(colorName)));
                }
                return result;
            }

            private static Color PhaseColor(string phaseId)
            {
                Color color;
                return phaseId != null &&
                    PhaseColors.TryGetValue(phaseId, out color)
                    ? color
                    : new Color(0.45f, 0.82f, 0.93f);
            }

            private static string Colorize(string text, Color color)
            {
                return "<color=#" +
                    ColorUtility.ToHtmlStringRGB(color) +
                    ">" + text + "</color>";
            }

            private static string Escape(string text)
            {
                return (text ?? "")
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
            }
        }
    }
}
