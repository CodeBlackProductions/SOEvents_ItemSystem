using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public static class UITKFactory
{
    public static VisualElement CreateNewUIWindow(
        string _UIName,
        Vector2 _ScreenPos,
        string _HeaderText,
        string _BodyText,
        Color _TextColor,
        Color _HyperlinkColor,
        Color _ValueColor,
        int _FontSize,
        UIDocument _UiDocument,
        System.Action<PointerUpLinkTagEvent> _HyplerinkClickedCallback,
        System.Action<PointerOverLinkTagEvent> _HyplerinkHoveredCallback,
        System.Action<PointerOutLinkTagEvent> _HyplerinkStopHoveredCallback,
        string _LinkID,
          System.Action<string> _WindowClosedCallback
        )
    {
        VisualElement root = _UiDocument.rootVisualElement;

        VisualElement window = new VisualElement();
        window.name = _UIName;
        window.style.position = Position.Absolute;
        window.style.left = _ScreenPos.x;
        window.style.top = _ScreenPos.y;
        window.style.flexDirection = FlexDirection.Column;
        window.style.paddingLeft = 0;
        window.style.paddingRight = 0;
        window.style.paddingTop = 0;
        window.style.paddingBottom = 0;
        window.style.backgroundColor = new StyleColor(Color.gray);
        window.style.borderTopWidth = 2;
        window.style.borderBottomWidth = 2;
        window.style.borderLeftWidth = 2;
        window.style.borderRightWidth = 2;
        window.style.borderTopColor = Color.black;
        window.style.borderBottomColor = Color.black;
        window.style.borderLeftColor = Color.black;
        window.style.borderRightColor = Color.black;
        window.style.maxWidth = Length.Percent(60);
        window.style.maxHeight = Length.Percent(80);

        VisualElement headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.justifyContent = Justify.SpaceBetween;
        headerRow.style.alignItems = Align.Center;
        headerRow.style.paddingLeft = 8;
        headerRow.style.paddingRight = 8;
        headerRow.style.paddingTop = 4;
        headerRow.style.paddingBottom = 4;
        headerRow.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
        headerRow.style.borderTopLeftRadius = 6;
        headerRow.style.borderTopRightRadius = 6;
        headerRow.style.borderBottomWidth = 1;
        headerRow.style.borderBottomColor = new StyleColor(Color.black);
        headerRow.style.flexShrink = 0;
        headerRow.AddManipulator(new DragManipulator(() => window));

        Label headerText = new Label(ParseRichText(_HeaderText, _HyperlinkColor, _ValueColor));
        headerText.style.unityFontStyleAndWeight = FontStyle.Bold;
        headerText.style.fontSize = _FontSize * 1.2f;
        headerText.style.color = _TextColor;
        headerText.style.flexGrow = 1;
        headerText.style.flexShrink = 1;
        headerText.style.marginRight = 6;
        headerText.RegisterCallback<PointerUpLinkTagEvent>(_HyplerinkClickedCallback.Invoke);
        headerText.RegisterCallback<PointerOverLinkTagEvent>(_HyplerinkHoveredCallback.Invoke);
        headerText.RegisterCallback<PointerOutLinkTagEvent>(_HyplerinkStopHoveredCallback.Invoke);

        Button btn_CloseWindow = new Button(() =>
        {
            _WindowClosedCallback.Invoke(_LinkID);
            window.RemoveFromHierarchy();
        })
        {
            text = "X"
        };
        btn_CloseWindow.style.alignSelf = Align.Stretch;
        btn_CloseWindow.style.flexShrink = 0;
        btn_CloseWindow.style.backgroundColor = Color.red;
        btn_CloseWindow.style.borderBottomColor = Color.black;
        btn_CloseWindow.style.borderTopColor = Color.black;
        btn_CloseWindow.style.borderLeftColor = Color.black;
        btn_CloseWindow.style.borderRightColor = Color.black;
        btn_CloseWindow.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            float width = btn_CloseWindow.resolvedStyle.width;
            btn_CloseWindow.style.height = width;
        });

        headerRow.Add(headerText);
        headerRow.Add(btn_CloseWindow);
        window.Add(headerRow);

        Label bodyText = new Label(ParseRichText(_BodyText, _HyperlinkColor, _ValueColor));
        bodyText.style.whiteSpace = WhiteSpace.Normal;
        bodyText.style.unityTextAlign = TextAnchor.UpperLeft;
        bodyText.style.fontSize = _FontSize;
        bodyText.style.color = _TextColor;
        bodyText.style.flexShrink = 0;
        bodyText.style.flexGrow = 0;
        bodyText.style.marginLeft = 10;
        bodyText.style.marginRight = 10;
        bodyText.style.marginTop = 10;
        bodyText.style.marginBottom = 10;
        bodyText.RegisterCallback<PointerUpLinkTagEvent>(_HyplerinkClickedCallback.Invoke);
        bodyText.RegisterCallback<PointerOverLinkTagEvent>(_HyplerinkHoveredCallback.Invoke);
        bodyText.RegisterCallback<PointerOutLinkTagEvent>(_HyplerinkStopHoveredCallback.Invoke);

        ScrollView scrollView = new ScrollView();
        scrollView.style.maxHeight = Length.Percent(80);
        scrollView.style.flexGrow = 0;
        scrollView.style.flexShrink = 1;
        scrollView.style.height = StyleKeyword.Auto;
        scrollView.style.unityOverflowClipBox = OverflowClipBox.ContentBox;

        scrollView.Add(bodyText);
        window.Add(scrollView);

        VisualElement resizeHandleBR = new VisualElement();
        resizeHandleBR.style.width = Length.Percent(10);
        resizeHandleBR.style.height = Length.Percent(10);
        resizeHandleBR.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        resizeHandleBR.style.position = Position.Absolute;
        resizeHandleBR.style.right = 0;
        resizeHandleBR.style.bottom = 0;
        resizeHandleBR.AddManipulator(new ResizeManipulator(() => window));
        window.Add(resizeHandleBR);

        VisualElement resizeHandleBL = new VisualElement();
        resizeHandleBL.style.width = Length.Percent(10);
        resizeHandleBL.style.height = Length.Percent(10);
        resizeHandleBL.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        resizeHandleBL.style.position = Position.Absolute;
        resizeHandleBL.style.left = 0;
        resizeHandleBL.style.bottom = 0;
        resizeHandleBL.AddManipulator(new ResizeManipulator(() => window));
        window.Add(resizeHandleBL);

        EventCallback<GeometryChangedEvent> callback = null;

        callback = evt =>
        {
            window.UnregisterCallback<GeometryChangedEvent>(callback);

            float width = evt.newRect.width;
            float height = evt.newRect.height;

            window.style.minWidth = new Length(width, LengthUnit.Pixel);
            window.style.minHeight = new Length(height, LengthUnit.Pixel);
        };

        window.RegisterCallback<GeometryChangedEvent>(callback);

        var scaler = new ResponsiveScaler();
        scaler.Setup(window);

        root.Add(window);

        return window;
    }

    public static VisualElement CreateNewUIPopup(
       string _UIName,
       Vector2 _ScreenPos,
       string _HeaderText,
       string _BodyText,
       Color _TextColor,
       Color _HyperlinkColor,
       Color _ValueColor,
       int _FontSize,
       UIDocument _UiDocument)
    {
        VisualElement root = _UiDocument.rootVisualElement;

        VisualElement window = new VisualElement();
        window.name = _UIName;
        window.style.position = Position.Absolute;
        window.style.left = _ScreenPos.x;
        window.style.top = _ScreenPos.y;
        window.style.flexDirection = FlexDirection.Column;
        window.style.paddingLeft = 0;
        window.style.paddingRight = 0;
        window.style.paddingTop = 0;
        window.style.paddingBottom = 0;
        window.style.backgroundColor = new StyleColor(Color.gray);
        window.style.borderTopWidth = 2;
        window.style.borderBottomWidth = 2;
        window.style.borderLeftWidth = 2;
        window.style.borderRightWidth = 2;
        window.style.borderTopColor = Color.black;
        window.style.borderBottomColor = Color.black;
        window.style.borderLeftColor = Color.black;
        window.style.borderRightColor = Color.black;
        window.style.maxWidth = Length.Percent(60);
        window.style.maxHeight = Length.Percent(80);

        VisualElement headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.justifyContent = Justify.SpaceBetween;
        headerRow.style.alignItems = Align.Center;
        headerRow.style.paddingLeft = 8;
        headerRow.style.paddingRight = 8;
        headerRow.style.paddingTop = 4;
        headerRow.style.paddingBottom = 4;
        headerRow.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
        headerRow.style.borderTopLeftRadius = 6;
        headerRow.style.borderTopRightRadius = 6;
        headerRow.style.borderBottomWidth = 1;
        headerRow.style.borderBottomColor = new StyleColor(Color.black);
        headerRow.style.flexShrink = 0;
        headerRow.AddManipulator(new DragManipulator(() => window));

        Label headerText = new Label(ParseRichText(_HeaderText, _HyperlinkColor, _ValueColor));
        headerText.style.unityFontStyleAndWeight = FontStyle.Bold;
        headerText.style.fontSize = _FontSize * 1.2f;
        headerText.style.color = _TextColor;
        headerText.style.flexGrow = 1;
        headerText.style.flexShrink = 1;
        headerText.style.marginRight = 6;

        headerRow.Add(headerText);

        window.Add(headerRow);

        Label bodyText = new Label(ParseRichText(_BodyText, _HyperlinkColor, _ValueColor));
        bodyText.style.whiteSpace = WhiteSpace.Normal;
        bodyText.style.unityTextAlign = TextAnchor.UpperLeft;
        bodyText.style.fontSize = _FontSize;
        bodyText.style.color = _TextColor;
        bodyText.style.flexShrink = 0;
        bodyText.style.flexGrow = 0;
        bodyText.style.marginLeft = 10;
        bodyText.style.marginRight = 10;
        bodyText.style.marginTop = 10;
        bodyText.style.marginBottom = 10;

        ScrollView scrollView = new ScrollView();
        scrollView.style.maxHeight = Length.Percent(80);
        scrollView.style.flexGrow = 0;
        scrollView.style.flexShrink = 1;
        scrollView.style.height = StyleKeyword.Auto;
        scrollView.style.unityOverflowClipBox = OverflowClipBox.ContentBox;

        scrollView.Add(bodyText);
        window.Add(scrollView);

        EventCallback<GeometryChangedEvent> callback = null;

        callback = evt =>
        {
            window.UnregisterCallback<GeometryChangedEvent>(callback);

            float width = evt.newRect.width;
            float height = evt.newRect.height;

            window.style.minWidth = new Length(width, LengthUnit.Pixel);
            window.style.minHeight = new Length(height, LengthUnit.Pixel);
        };

        window.RegisterCallback<GeometryChangedEvent>(callback);

        root.Add(window);

        return window;
    }

    private static string ParseRichText(string _Text, Color _HyperlinColor, Color _ValueColor)
    {
        string result = _Text;
        result = CreateHyperlink(result, _HyperlinColor);
        result = ImportValues(result, _ValueColor);

        return result;
    }

    private static string CreateHyperlink(string _Text, Color _Color)
    {
        string text = _Text;
        List<string> substrings = GetSubstringsInBraces(text, '{', '}');

        foreach (string subTTID in substrings)
        {
            string subTTDisplay = UIToolTipRegistry.Instance.RetrieveTooltipHyperlinkText(subTTID);
            text = text.Replace("{" + subTTID + "}", $"<link=\"{subTTID}\"><color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(_Color)}>{subTTDisplay}</color></link>");
        }

        return text;
    }

    private static string ImportValues(string _Text, Color _ValueColor)
    {
        string text = _Text;
        List<string> substrings = GetSubstringsInBraces(text, '[', ']');

        foreach (string subTTID in substrings)
        {
            List<string> subTTIDKeys = GetSubstringsInBraces(text, '(', ')');
            ScriptableObject item = null;
            string propName = null;
            string statName = null;

            foreach (string subTTIDKey in subTTIDKeys)
            {
                if (subTTIDKey.Contains("Module:"))
                {
                    string moduleName = subTTIDKey.Replace("Module:", "");
                    item = LoadAssetByName<ScriptableObject>(moduleName);
                }
                else if (subTTIDKey.Contains("Property:"))
                {
                    propName = subTTIDKey.Replace("Property:", "");
                }
                else if (subTTIDKey.Contains("Stat:"))
                {
                    statName = subTTIDKey.Replace("Stat:", "");
                }
            }

            if (item != null && !string.IsNullOrEmpty(propName))
            {
                var propInfo = item.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null)
                {
                    object propValue = propInfo.GetValue(item);
                    text = text.Replace("[" + subTTID + "]", $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(_ValueColor)}>{propValue?.ToString() ?? string.Empty}</color>");
                }
                else
                {
                    Debug.LogWarning($"Property '{propName}' not found in item '{item.name}'.");
                }
            }
            else if (item != null && !string.IsNullOrEmpty(statName))
            {
                var propInfo = item.GetType().GetProperty("Stats", BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null)
                {
                    SO_Stat stat;
                    Dictionary<string, SO_Stat> propValue = propInfo.GetValue(item) as Dictionary<string, SO_Stat>;
                    propValue.TryGetValue(statName, out stat);

                    if (stat != null)
                    {
                        text = text.Replace("[" + subTTID + "]", $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(_ValueColor)}>{stat.GetStatValue()?.ToString() ?? string.Empty}</color>");
                    }
                    else
                    {
                        Debug.LogWarning($"Stat '{statName}' not found in item '{item.name}'.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Stat Dictionary not found in item '{item.name}'.");
                }
            }
            else
            {
                Debug.LogWarning($"Item or property not found for ID '{subTTID}'.");
            }
        }

        return text;
    }

    private static List<string> GetSubstringsInBraces(string _Input, char _MarkerOpen, char _MarkerClose)
    {
        string matchString = @"\{([^}]*)\}";
        matchString = matchString.Replace("{", _MarkerOpen.ToString());
        matchString = matchString.Replace("}", _MarkerClose.ToString());
        var matches = Regex.Matches(_Input, matchString);
        var results = new List<string>();
        foreach (Match match in matches)
        {
            results.Add(match.Groups[1].Value);
        }
        return results;
    }

    private static T LoadAssetByName<T>(string _AssetName) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets(" t:" + typeof(T).Name);
        if (guids.Length > 0)
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset != null && asset is IItemModule && (asset as IItemModule).ModuleName == _AssetName)
                {
                    return asset as T;
                }
            }
        }

        Debug.LogError($"Asset '{_AssetName}' of type {typeof(T).Name} not found.");
        return null;
    }
}

public class DragManipulator : PointerManipulator
{
    private Vector2 m_StartMousePosition;
    private Vector2 m_StartElementPosition;
    private VisualElement m_Window;
    private System.Func<VisualElement> m_GetWindow;

    public DragManipulator(System.Func<VisualElement> _GetWindow)
    {
        m_GetWindow = _GetWindow;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnPointerDown(PointerDownEvent _Event)
    {
        m_Window = m_GetWindow.Invoke();
        if (m_Window == null)
        {
            return;
        }

        m_StartMousePosition = _Event.position;

        m_StartElementPosition = new Vector2(
            m_Window.resolvedStyle.left,
            m_Window.resolvedStyle.top
        );

        target.CapturePointer(_Event.pointerId);
        _Event.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent _Event)
    {
        if (target.HasPointerCapture(_Event.pointerId))
        {
            m_Window = m_GetWindow.Invoke();
            if (m_Window == null)
            {
                return;
            }

            Vector2 delta = (Vector2)_Event.position - m_StartMousePosition;

            float newX = m_StartElementPosition.x + delta.x;
            float newY = m_StartElementPosition.y + delta.y;

            m_Window.style.left = newX;
            m_Window.style.top = newY;

            _Event.StopPropagation();
        }
    }

    private void OnPointerUp(PointerUpEvent _Event)
    {
        if (target.HasPointerCapture(_Event.pointerId))
        {
            target.ReleasePointer(_Event.pointerId);
            _Event.StopPropagation();
        }
    }
}

public class ResizeManipulator : PointerManipulator
{
    private VisualElement m_Window;
    private Vector2 m_StartMousePosition;
    private Vector2 m_StartWindowPosition;
    private Vector2 m_StartSize;
    private Vector2 m_ResizeDir;
    private System.Func<VisualElement> m_GetWindow;

    public ResizeManipulator(System.Func<VisualElement> _GetWindow)
    {
        m_GetWindow = _GetWindow;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnPointerDown(PointerDownEvent _Event)
    {
        m_Window = m_GetWindow.Invoke();
        if (m_Window == null)
        {
            return;
        }

        m_StartMousePosition = _Event.position;
        m_StartSize = new Vector2(m_Window.resolvedStyle.width, m_Window.resolvedStyle.height);
        m_StartWindowPosition = new Vector2(
           m_Window.resolvedStyle.left,
           m_Window.resolvedStyle.top
        );

        Rect windowRect = m_Window.worldBound;
        Rect handleRect = target.worldBound;

        Vector2 handleCenter = new Vector2(
            handleRect.center.x,
            handleRect.center.y
        );

        Vector2 windowCenter = new Vector2(
            windowRect.center.x,
            windowRect.center.y
        );

        Vector2 dir = handleCenter - windowCenter;

        m_ResizeDir = new Vector2(
            Mathf.Abs(dir.x) < 1f ? 0 : Mathf.Sign(dir.x),
            Mathf.Abs(dir.y) < 1f ? 0 : Mathf.Sign(dir.y)
        );

        target.CapturePointer(_Event.pointerId);
        _Event.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent _Event)
    {
        if (target.HasPointerCapture(_Event.pointerId))
        {
            m_Window = m_GetWindow.Invoke();
            if (m_Window == null)
            {
                return;
            }

            Vector2 mouseDelta = (Vector2)_Event.position - m_StartMousePosition;

            Vector2 deltaSize = new Vector2(
                mouseDelta.x * m_ResizeDir.x,
                mouseDelta.y * m_ResizeDir.y
            );

            Vector2 newSize = m_StartSize + deltaSize;

            newSize.x = Mathf.Max(newSize.x, m_Window.style.minWidth.value.value);
            newSize.y = Mathf.Max(newSize.y, m_Window.style.minHeight.value.value);

            Vector2 actualChange = newSize - m_StartSize;

            if (actualChange.x != 0 || actualChange.y != 0)
            {
                Vector2 offset = new Vector2(
               (m_ResizeDir.x < 0) ? actualChange.x : 0,
               (m_ResizeDir.y < 0) ? actualChange.y : 0
                );

                m_Window.style.width = newSize.x;
                m_Window.style.height = newSize.y;

                m_Window.style.left = m_StartWindowPosition.x - offset.x;
                m_Window.style.top = m_StartWindowPosition.y - offset.y;
            }

            _Event.StopPropagation();
        }
    }

    private void OnPointerUp(PointerUpEvent _Event)
    {
        if (target.HasPointerCapture(_Event.pointerId))
        {
            target.ReleasePointer(_Event.pointerId);
            _Event.StopPropagation();
        }
    }
}

public class ResponsiveScaler
{
    private Vector2? m_InitialSize = null;
    private Dictionary<VisualElement, ElementBaseStyle> m_OriginalStyles = new();

    public void Setup(VisualElement window)
    {
        window.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            float width = window.resolvedStyle.width;
            float height = window.resolvedStyle.height;

            if (m_InitialSize == null)
                m_InitialSize = new Vector2(width, height);

            Vector2 initial = m_InitialSize.Value;
            float scaleX = width / initial.x;
            float scaleY = height / initial.y;
            float scale = Mathf.Min(scaleX, scaleY);

            ApplyScale(window, scale);
        });
    }

    private void ApplyScale(VisualElement element, float scale)
    {
        // Cache original styles once
        if (!m_OriginalStyles.ContainsKey(element))
        {
            m_OriginalStyles[element] = new ElementBaseStyle
            {
                fontSize = element.resolvedStyle.fontSize,
                paddingTop = element.resolvedStyle.paddingTop,
                paddingBottom = element.resolvedStyle.paddingBottom,
                paddingLeft = element.resolvedStyle.paddingLeft,
                paddingRight = element.resolvedStyle.paddingRight,
                width = element.resolvedStyle.width,
                height = element.resolvedStyle.height
            };
        }

        var baseStyle = m_OriginalStyles[element];

        // Apply to Label
        if (element is Label label)
        {
            label.style.fontSize = baseStyle.fontSize * scale;
        }

        // Apply to Button
        if (element is Button button)
        {
            button.style.fontSize = baseStyle.fontSize * scale;
            button.style.paddingTop = baseStyle.paddingTop * scale;
            button.style.paddingBottom = baseStyle.paddingBottom * scale;
            button.style.paddingLeft = baseStyle.paddingLeft * scale;
            button.style.paddingRight = baseStyle.paddingRight * scale;
        }

        // Apply to Image
        if (element is Image image)
        {
            image.style.width = baseStyle.width * scale;
            image.style.height = baseStyle.height * scale;
        }

        // Recursively scale children
        foreach (var child in element.Children())
        {
            ApplyScale(child, scale);
        }
    }

    private class ElementBaseStyle
    {
        public float fontSize;
        public float paddingTop, paddingBottom, paddingLeft, paddingRight;
        public float width, height;
    }
}