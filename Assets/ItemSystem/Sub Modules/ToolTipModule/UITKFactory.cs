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
        bool _AddControlElements = false,
        System.Action<PointerUpLinkTagEvent> _HyplerinkClickedCallback = null,
        System.Action<PointerOverLinkTagEvent> _HyplerinkHoveredCallback = null,
        System.Action<PointerOutLinkTagEvent> _HyplerinkStopHoveredCallback = null,
        string _LinkID = null,
        System.Action<string> _WindowClosedCallback = null,
        Texture2D _HeaderBGImage = null,
        Texture2D _WindowBGImage = null,
        Texture2D _CloseWindowButtonImage = null
        )
    {
        VisualElement root = _UiDocument.rootVisualElement;

        VisualElement window = CreateMainWindow(_UIName, _ScreenPos, _WindowBGImage);

        VisualElement header = CreateHeader(
            _AddControlElements,
            window,
            _HeaderText,
            _TextColor,
            _HyperlinkColor,
            _ValueColor,
            _FontSize,
            _HeaderBGImage,
            _CloseWindowButtonImage,
            _HyplerinkClickedCallback,
            _HyplerinkHoveredCallback,
            _HyplerinkStopHoveredCallback,
            _LinkID,
            _WindowClosedCallback);

        window.Add(header);

        VisualElement body = CreateBody(
            _AddControlElements,
            window,
            _BodyText,
            _TextColor,
            _HyperlinkColor,
            _ValueColor,
            _FontSize,
            _HyplerinkClickedCallback,
            _HyplerinkHoveredCallback,
            _HyplerinkStopHoveredCallback
            );

        window.Add(body);

        RegisterMinSizeSnapshot(window);

        if (_AddControlElements)
        {
            VisualElement resizeHandle_BL = CreateResizeHandle(window, new Vector2(0, 0));
            window.Add(resizeHandle_BL);

            VisualElement resizeHandle_BR = CreateResizeHandle(window, new Vector2(0.9f, 0));
            window.Add(resizeHandle_BR);

            var scaler = new ResponsiveScaler();
            scaler.Setup(window);
        }

        root.Add(window);

        return window;
    }

    public static string ParseRichText(string _Text, Color _HyperlinColor, Color _ValueColor)
    {
        string result = _Text;
        result = CreateHyperlink(result, _HyperlinColor);
        result = ImportValues(result, _ValueColor);

        return result;
    }

    #region Internal Methods

    #region BaseElementCreation

    private static VisualElement CreateMainWindow(string _UIName, Vector2 _ScreenPos, Texture2D _WindowBGImage = null)
    {
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

        if (_WindowBGImage == null)
        {
            window.style.backgroundColor = new StyleColor(Color.gray);
            window.style.borderTopWidth = 2;
            window.style.borderBottomWidth = 2;
            window.style.borderLeftWidth = 2;
            window.style.borderRightWidth = 2;
            window.style.borderTopColor = Color.black;
            window.style.borderBottomColor = Color.black;
            window.style.borderLeftColor = Color.black;
            window.style.borderRightColor = Color.black;
        }
        else
        {
            window.style.backgroundImage = _WindowBGImage;
        }

        window.style.maxWidth = Length.Percent(90);
        window.style.maxHeight = Length.Percent(90);

        return window;
    }

    private static VisualElement CreateHeader(
        bool _AddControlElements,
        VisualElement _MainWindow,
        string _HeaderText,
        Color _TextColor,
        Color _HyperlinkColor,
        Color _ValueColor,
        float _FontSize,
        Texture2D _HeaderBGImage = null,
         Texture2D _CloseWindowButtonImage = null,
        System.Action<PointerUpLinkTagEvent> _HyplerinkClickedCallback = null,
        System.Action<PointerOverLinkTagEvent> _HyplerinkHoveredCallback = null,
        System.Action<PointerOutLinkTagEvent> _HyplerinkStopHoveredCallback = null,
        string _LinkID = null,
        System.Action<string> _WindowClosedCallback = null
        )
    {
        VisualElement headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.justifyContent = Justify.SpaceBetween;
        headerRow.style.alignItems = Align.Center;
        headerRow.style.paddingLeft = 8;
        headerRow.style.paddingRight = 8;
        headerRow.style.paddingTop = 4;
        headerRow.style.paddingBottom = 4;

        if (_HeaderBGImage == null)
        {
            headerRow.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            headerRow.style.borderTopLeftRadius = 6;
            headerRow.style.borderTopRightRadius = 6;
            headerRow.style.borderBottomWidth = 1;
            headerRow.style.borderBottomColor = new StyleColor(Color.black);
        }
        else
        {
            headerRow.style.backgroundImage = _HeaderBGImage;
        }

        headerRow.style.flexShrink = 0;

        Label headerText = CreateLabel(_HeaderText, _TextColor, _HyperlinkColor, _ValueColor, _FontSize, 1.2f, FontStyle.Bold);

        headerRow.Add(headerText);

        if (_AddControlElements)
        {
            headerRow.AddManipulator(new DragManipulator(() => _MainWindow));

            RegisterLinkCallbacks(headerText, _HyplerinkClickedCallback, _HyplerinkHoveredCallback, _HyplerinkStopHoveredCallback);

            if (_WindowClosedCallback != null && _LinkID != null)
            {
                headerRow.Add(CreateCloseButton(_MainWindow, _CloseWindowButtonImage, _LinkID, _WindowClosedCallback));
            }
        }

        return headerRow;
    }

    private static VisualElement CreateBody(
        bool _AddControlElements,
        VisualElement _MainWindow,
        string _BodyText,
        Color _TextColor,
        Color _HyperlinkColor,
        Color _ValueColor,
        float _FontSize,
        System.Action<PointerUpLinkTagEvent> _HyplerinkClickedCallback = null,
        System.Action<PointerOverLinkTagEvent> _HyplerinkHoveredCallback = null,
        System.Action<PointerOutLinkTagEvent> _HyplerinkStopHoveredCallback = null
        )
    {
        Label bodyText = CreateLabel(_BodyText, _TextColor, _HyperlinkColor, _ValueColor, _FontSize);

        ScrollView scrollView = CreateScrollView(bodyText);

        if (_AddControlElements)
        {
            RegisterLinkCallbacks(bodyText, _HyplerinkClickedCallback, _HyplerinkHoveredCallback, _HyplerinkStopHoveredCallback);
        }

        return scrollView;
    }

    private static Label CreateLabel(string _Text, Color _TextColor, Color _HyperlinkColor, Color _ValueColor, float _FontSize, float _FontsizeFactor = 1, FontStyle _FontStyle = FontStyle.Normal)
    {
        return new Label(ParseRichText(_Text, _HyperlinkColor, _ValueColor))
        {
            style =
            {
                whiteSpace = WhiteSpace.Normal,
                unityTextAlign = TextAnchor.UpperLeft,
                fontSize = _FontSize * _FontsizeFactor,
                color = _TextColor,
                flexShrink = 0,
                flexGrow = 0,
                marginLeft = 10,
                marginRight = 10,
                marginTop = 10,
                marginBottom = 10
            }
        };
    }

    private static ScrollView CreateScrollView(Label _Label)
    {
        var scrollView = new ScrollView
        {
            style =
            {
                maxHeight = Length.Percent(80),
                flexGrow = 0,
                flexShrink = 1,
                height = StyleKeyword.Auto,
                unityOverflowClipBox = OverflowClipBox.ContentBox
            }
        };
        scrollView.Add(_Label);
        return scrollView;
    }

    #endregion BaseElementCreation

    #region FunctionalElementCreation

    private static void RegisterLinkCallbacks(
        Label _Text,
        System.Action<PointerUpLinkTagEvent> _HyplerinkClickedCallback = null,
        System.Action<PointerOverLinkTagEvent> _HyplerinkHoveredCallback = null,
        System.Action<PointerOutLinkTagEvent> _HyplerinkStopHoveredCallback = null
        )
    {
        if (_HyplerinkClickedCallback != null)
        {
            _Text.RegisterCallback<PointerUpLinkTagEvent>(_HyplerinkClickedCallback.Invoke);
        }
        if (_HyplerinkHoveredCallback != null)
        {
            _Text.RegisterCallback<PointerOverLinkTagEvent>(_HyplerinkHoveredCallback.Invoke);
        }
        if (_HyplerinkStopHoveredCallback != null)
        {
            _Text.RegisterCallback<PointerOutLinkTagEvent>(_HyplerinkStopHoveredCallback.Invoke);
        }
    }

    private static Button CreateCloseButton(VisualElement _MainWindow, Texture2D _CloseWindowButtonImage = null, string _LinkID = null, System.Action<string> _WindowClosedCallback = null)
    {
        Button btn_CloseWindow = new Button(() =>
        {
            _WindowClosedCallback.Invoke(_LinkID);
            _MainWindow.RemoveFromHierarchy();
        })
        {
            text = "X"
        };
        btn_CloseWindow.style.alignSelf = Align.Stretch;
        btn_CloseWindow.style.flexShrink = 0;

        if (_CloseWindowButtonImage == null)
        {
            btn_CloseWindow.style.backgroundColor = Color.red;
            btn_CloseWindow.style.borderBottomColor = Color.black;
            btn_CloseWindow.style.borderTopColor = Color.black;
            btn_CloseWindow.style.borderLeftColor = Color.black;
            btn_CloseWindow.style.borderRightColor = Color.black;
        }
        else
        {
            btn_CloseWindow.style.backgroundImage = _CloseWindowButtonImage;
            btn_CloseWindow.text = "";
        }

        btn_CloseWindow.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            float width = btn_CloseWindow.resolvedStyle.width;
            btn_CloseWindow.style.height = width;
        });

        return btn_CloseWindow;
    }

    private static void RegisterMinSizeSnapshot(VisualElement _Window)
    {
        EventCallback<GeometryChangedEvent> callback = null;

        callback = evt =>
        {
            _Window.UnregisterCallback(callback);

            float width = evt.newRect.width;
            float height = evt.newRect.height;

            _Window.style.minWidth = new Length(width, LengthUnit.Pixel);
            _Window.style.minHeight = new Length(height, LengthUnit.Pixel);
        };

        _Window.RegisterCallback<GeometryChangedEvent>(callback);
    }

    private static VisualElement CreateResizeHandle(VisualElement _MainWindow, Vector2 _RelativePosition)
    {
        VisualElement resizeHandle = new VisualElement();
        resizeHandle.style.width = Length.Percent(10);
        resizeHandle.style.height = Length.Percent(10);
        resizeHandle.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        resizeHandle.style.position = Position.Absolute;

        resizeHandle.style.left = Length.Percent(_RelativePosition.x * 100);
        resizeHandle.style.bottom = Length.Percent(_RelativePosition.y * 100);

        resizeHandle.AddManipulator(new ResizeManipulator(() => _MainWindow));

        return resizeHandle;
    }

    #endregion FunctionalElementCreation

    #region ItemSystemLoading

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
                        if (stat is SO_Stat_StaticValue staticValue)
                        {
                            text = text.Replace("[" + subTTID + "]", $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(_ValueColor)}>{staticValue.GetStatValue()?.ToString() ?? string.Empty}</color>");
                        }
                        else if (stat is SO_Stat_DynamicValue value)
                        {
                            int index = 0;
                            var indexPropInfo = item.GetType().GetProperty("StatIndices", BindingFlags.Public | BindingFlags.Instance);
                            if (indexPropInfo != null)
                            {
                                Dictionary<string, int> indexPropValue = indexPropInfo.GetValue(item) as Dictionary<string, int>;
                                indexPropValue.TryGetValue(statName, out index);

                                text = text.Replace("[" + subTTID + "]", $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(_ValueColor)}>{value.GetStatValue(index) ?? string.Empty}</color>");
                            }
                            else
                            {
                                Debug.LogWarning($"Property '{propName}' not found in item '{item.name}'.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Stat '{statName}' not found in item '{item.name}'.");
                        }
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

    #endregion ItemSystemLoading

    #endregion Internal Methods
}