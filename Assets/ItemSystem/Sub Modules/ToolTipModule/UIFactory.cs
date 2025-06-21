using ItemSystem.MainModule;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSystem.SubModules
{
    /// <summary>
    /// Static class to create and manage UI windows with tooltips and hyperlinks.
    /// </summary>
    public static class UIFactory
    {
        public static GameObject CreateNewUIWindow(string _UIName, Vector3 _MousePos, Vector2 _ScreenPercent, string _HeaderText, string _Text, Color _TextColor, Color _HyperlinkColor, Color _ValueColor, int _FontSize)
        {
            Vector2 screenPos = new Vector2(_MousePos.x / Screen.width, _MousePos.y / Screen.height);

            GameObject newUI = CreateNewUIWindow(_UIName, screenPos, _ScreenPercent, _HeaderText, _Text, _TextColor, _HyperlinkColor, _ValueColor, _FontSize);

            return newUI;
        }

        public static GameObject CreateNewUIWindow(string _UIName, Vector2 _ScreenPos, Vector2 _ScreenPercent, string _HeaderText, string _Text, Color _TextColor, Color _HyperlinkColor, Color _ValueColor, int _FontSize)
        {
            GameObject newUI = new GameObject(_UIName);
            Canvas canvas = newUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            newUI.AddComponent<GraphicRaycaster>();

            GameObject uiContainer = new GameObject(_UIName + "_Container");
            uiContainer.transform.SetParent(canvas.transform, false);
            RectTransform rt = uiContainer.AddComponent<RectTransform>();
            ResetRectTransform(uiContainer);
            SetSizeByScreenPercentage(rt, _ScreenPercent, _ScreenPos);
            rt.anchoredPosition = Vector2.zero;

            CreateUIElement<Image>(_UIName, "Background_Border", uiContainer, null, Color.black, new Vector2(0f, 0f), new Vector2(1f, 1f), true, false);
            CreateUIElement<Image>(_UIName, "Background", uiContainer, null, Color.gray, new Vector2(0.01f, 0.01f), new Vector2(0.99f, 0.99f), true, false);

            GameObject textObj = CreateNewTextElement(_UIName, _Text, _TextColor, _HyperlinkColor, _ValueColor, _FontSize, new Vector2(0.025f, 0.1f), new Vector2(0.95f, 0.85f), true);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(uiContainer.transform, false);
            ResetRectTransform(textObj);
            text.transform.SetAsLastSibling();

            Button btn_TopBorder = CreateUIElement<Button>(_UIName, "TopBorder", uiContainer, null, Color.black, new Vector2(0f, 0.9f), new Vector2(1f, 1f), true, true).GetComponent<Button>();
            AddDragFunctionality(btn_TopBorder, rt);

            GameObject headerObj = CreateNewTextElement(_UIName + "_Header_", _HeaderText, Color.white, _HyperlinkColor, _ValueColor, 18, new Vector2(0.02f, 0.92f), new Vector2(0.98f, 0.98f), false);
            TextMeshProUGUI headerText = headerObj.GetComponent<TextMeshProUGUI>();
            headerText.transform.SetParent(uiContainer.transform, false);
            ResetRectTransform(headerObj);
            headerText.transform.SetAsLastSibling();

            Button btn_CloseWindow = CreateUIElement<Button>(_UIName, "CloseWindow", uiContainer, null, Color.red, new Vector2(0.94f, 0.92f), new Vector2(0.98f, 0.98f), true, true).GetComponent<Button>();
            btn_CloseWindow.onClick.AddListener(() =>
            {
                GameObject.Destroy(btn_CloseWindow.transform.parent.gameObject);
            });

            Button btn_Resize_R = CreateUIElement<Button>(_UIName, "Resize_LR", uiContainer, null, Color.green, new Vector2(0.94f, 0.02f), new Vector2(0.98f, 0.08f), true, true).GetComponent<Button>();
            AddResizeFunctionality(btn_Resize_R, rt, rt.rect.size);

            Button btn_Resize_L = CreateUIElement<Button>(_UIName, "Resize_LL", uiContainer, null, Color.green, new Vector2(0.04f, 0.02f), new Vector2(0.08f, 0.08f), true, true).GetComponent<Button>();
            AddResizeFunctionality(btn_Resize_L, rt, rt.rect.size);

            return newUI;
        }

        public static GameObject CreateNewUIFromPrefab(GameObject _Prefab, string _UIName, Vector3 _MousePos, Vector2 _Pivot, string _HeaderText, string _Text, Color _HyperlinkColor, Color _ValueColor)
        {
            Vector2 _ScreenPos = new Vector2(_MousePos.x / Screen.width, _MousePos.y / Screen.height);

            GameObject newUI = CreateNewUIFromPrefab(_Prefab, _UIName, _ScreenPos, _Pivot, _HeaderText, _Text, _HyperlinkColor, _ValueColor);

            return newUI;
        }

        public static GameObject CreateNewUIFromPrefab(GameObject _Prefab, string _UIName, Vector2 _ScreenPos, Vector2 _Pivot, string _HeaderText, string _Text, Color _HyperlinkColor, Color _ValueColor)
        {
            GameObject newUI = GameObject.Instantiate(_Prefab);
            newUI.name = _UIName;

            GameObject uiContainer = newUI.GetComponentsInChildren<Transform>().Where((t, b) => { return t.name.Contains("_OBJContent"); }).FirstOrDefault()?.gameObject;
            RectTransform rt = null;
            if (uiContainer == null)
            {
                Debug.LogError($"UI Container (GameObject) with name containing '_OBJContent' not found in prefab {_UIName}. Please ensure the prefab has a valid UI Container.");
            }
            else
            {
                rt = uiContainer.GetComponent<RectTransform>();

                if (rt == null)
                {
                    Debug.LogError($"RectTransform not found in UI Container of prefab {_UIName}. Please ensure the prefab has a valid RectTransform component.");
                }
            }

            TextMeshProUGUI ContentText = newUI.GetComponentsInChildren<TextMeshProUGUI>().Where((t, b) => { return t.name.Contains("_TMPContent"); }).FirstOrDefault();
            if (ContentText == null)
            {
                Debug.LogError($"TextMeshProUGUI with name containing '_TMPContent' not found in prefab {_UIName}. Please ensure the prefab has a valid TextMeshProUGUI component.");
            }
            else
            {
                ContentText.text = CreateHyperlink(_Text, _HyperlinkColor);
                ContentText.text = ImportValues(ContentText.text, _ValueColor);
            }

            IEnumerable<Button> btn_draggables = newUI.GetComponentsInChildren<Button>().Where((b, c) => { return b.name.Contains("_BTNDrag"); });
            foreach (Button btn in btn_draggables)
            {
                AddDragFunctionality(btn, rt);
            }

            TextMeshProUGUI headerText = newUI.GetComponentsInChildren<TextMeshProUGUI>().Where((t, b) => { return t.name.Contains("_TMPHeader"); }).FirstOrDefault();
            if (ContentText == null)
            {
                Debug.LogError($"TextMeshProUGUI with name containing '_TMPHeader' not found in prefab {_UIName}. Please ensure the prefab has a valid TextMeshProUGUI component.");
            }
            else
            {
                headerText.text = CreateHyperlink(_HeaderText, _HyperlinkColor);
                headerText.text = ImportValues(headerText.text, _ValueColor);
            }

            Button btn_CloseWindow = newUI.GetComponentsInChildren<Button>().Where((b, c) => { return b.name.Contains("_BTNClose"); }).FirstOrDefault();
            if (btn_CloseWindow == null)
            {
                Debug.LogError($"Button with name containing '_BTNClose' not found in prefab {_UIName}. Please ensure the prefab has a valid Button component.");
            }
            else
            {
                btn_CloseWindow.onClick.AddListener(() =>
                {
                    GameObject.Destroy(newUI.transform.gameObject);
                });
            }

            IEnumerable<Button> btn_resizables = newUI.GetComponentsInChildren<Button>().Where((b, c) => { return b.name.Contains("_BTNResize"); });
            foreach (Button btn in btn_resizables)
            {
                AddResizeFunctionality(btn, rt, rt.rect.size);
            }

            return newUI;
        }

        private static void SetSizeByScreenPercentage(RectTransform _Rt, Vector2 _ScreenPercent, Vector2 _Position)
        {
            _Rt.anchorMin = _Rt.anchorMax = _Position;
            _Rt.pivot = new Vector2(0.5f, 0.5f);

            float width = Screen.width * _ScreenPercent.x;
            float height = Screen.height * _ScreenPercent.y;

            _Rt.sizeDelta = new Vector2(width, height);
        }

        private static GameObject CreateUIElement<T>(string _UIName, string _ElementName, GameObject _Parent, Sprite _Image, Color _Color, Vector2 _MinAnchor, Vector2 _MaxAnchor, bool _IsRaycastTarget, bool _BringToFront) where T : UnityEngine.Component
        {
            GameObject newUIObject = new GameObject(_UIName + "_" + _ElementName);
            T newUIElement = newUIObject.AddComponent<T>();
            newUIElement.transform.SetParent(_Parent.transform, false);

            Image image;
            newUIElement.TryGetComponent<Image>(out image);

            if (image == null)
            {
                image = newUIElement.AddComponent<Image>();
            }

            if (_Image != null)
            {
                image.sprite = _Image;
            }

            image.color = _Color;

            image.rectTransform.anchorMin = _MinAnchor;
            image.rectTransform.anchorMax = _MaxAnchor;
            ResetRectTransform(newUIObject);

            image.raycastTarget = _IsRaycastTarget;

            if (_BringToFront)
            {
                newUIElement.transform.SetAsLastSibling();
            }

            return newUIObject;
        }

        private static void ResetRectTransform(GameObject _Object)
        {
            RectTransform rt = _Object.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        private static void AddDragFunctionality(Button _Button, RectTransform _RT)
        {
            EventTrigger trigger = _Button.gameObject.AddComponent<EventTrigger>();
            Vector2 dragOffset = Vector2.zero;
            EventTrigger.Entry beginDrag = new EventTrigger.Entry();
            beginDrag.eventID = EventTriggerType.BeginDrag;
            beginDrag.callback.AddListener((data) =>
            {
                Vector2 screenPos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
                dragOffset = screenPos - _RT.anchorMin;
            });
            EventTrigger.Entry drag = new EventTrigger.Entry();
            drag.eventID = EventTriggerType.Drag;
            drag.callback.AddListener((data) =>
            {
                Vector2 screenPos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
                Vector2 newPos = screenPos - dragOffset;
                _RT.anchorMin = _RT.anchorMax = newPos;
            });
            trigger.triggers.Add(beginDrag);
            trigger.triggers.Add(drag);
        }

        private static void AddResizeFunctionality(Button handleButton, RectTransform target, Vector2 _MinSize)
        {
            Vector2 startMousePos = Vector2.zero;
            Vector2 startSize = Vector2.zero;
            Vector2 startAnchoredPos = Vector2.zero;
            Vector2 resizeDir = (handleButton.transform.position - target.transform.position).normalized;

            EventTrigger trigger = handleButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry beginDrag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.BeginDrag
            };
            beginDrag.callback.AddListener((data) =>
            {
                startMousePos = Input.mousePosition;
                startSize = target.sizeDelta;
                startAnchoredPos = RectTransformUtility.PixelAdjustPoint(target.anchoredPosition, target.transform, target.parent.GetComponent<Canvas>());
            });

            EventTrigger.Entry drag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag
            };
            drag.callback.AddListener((data) =>
            {
                Vector2 mouseDelta = (Vector2)Input.mousePosition - startMousePos;

                Vector2 deltaSize = new Vector2(
                    mouseDelta.x * resizeDir.x,
                    mouseDelta.y * resizeDir.y
                );

                Vector2 newSize = startSize + deltaSize;

                newSize.x = Mathf.Max(_MinSize.x, newSize.x);
                newSize.y = Mathf.Max(_MinSize.y, newSize.y);

                Vector2 actualChange = newSize - startSize;

                Vector2 offset = new Vector2(
                    resizeDir.x * actualChange.x * 0.5f,
                    resizeDir.y * actualChange.y * 0.5f
                );

                target.sizeDelta = newSize;
                target.anchoredPosition = startAnchoredPos + offset;
            });

            trigger.triggers.Add(beginDrag);
            trigger.triggers.Add(drag);
        }

        public static GameObject CreateNewTextElement(string _UIName, string _Text, Color _TextColor, Color _HyperlinkColor, Color _ValueColor, int _FontSize, Vector2 _MinAnchor, Vector2 _MaxAnchor, bool _IsRaycastTarget)
        {
            GameObject textObj = new GameObject(_UIName + "_Text");
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = CreateHyperlink(_Text, _HyperlinkColor);
            text.text = ImportValues(text.text, _ValueColor);
            text.fontSize = _FontSize;
            text.color = _TextColor;
            text.rectTransform.anchorMin = _MinAnchor;
            text.rectTransform.anchorMax = _MaxAnchor;
            text.richText = true;
            text.raycastTarget = _IsRaycastTarget;

            return textObj;
        }

        public static string CreateHyperlink(string _Text, Color _Color)
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

        public static string ImportValues(string _Text, Color _ValueColor)
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
}